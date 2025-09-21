using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ATLAMod;
using ATLAMod.Buffs.BendingStyles;
using ATLAMod.UI.BendingScroll;
using System.Security.Cryptography.X509Certificates;
using Terraria.GameInput;
using ATLAMod.Buffs.FireBendingBuffs;
using ATLAMod.Systems;
using ATLAMod.Systems.Bending;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using ATLAMod.Systems.Players.Animation;
using Terraria.DataStructures;

namespace ATLAMod.Systems.Players
{
    public class BendingPlayer : ModPlayer
    {
        public enum BendingStyle { None, Fire, Water, Earth, Air }

        public BendingStyle chosenStyle = BendingStyle.None;

        public bool hasLearnedFire = false;
        public bool hasLearnedWater = false;
        public bool hasLearnedEarth = false;
        public bool hasLearnedAir = false;

        public bool hasChosenBending;

        //FIREBENDING BREATH METER
        public float maxBreath = 100;
        public float breath = 100;
        public bool takenBreath = false;
        public int breathRegenTimer = 0;
        public int breatheTimer = 60;
        public int breatheCooldownTimer = 0;
        public bool isActivelyBreathing = false;
        public int activeBreathingDuration = 0;
        public int breathShakeTicks = 0;
        public int breathFailSoundCooldown = 0;


        //ATTACKHOTBAR STUFF
        public struct MoveSlot
        {
            public bool Unlocked;
            public string MoveId;
        }
        public MoveSlot[] MoveSlots = new MoveSlot[6];
        public int UnlockedSlots = 2;
        public int SelectedSlotIndex = 0;
        public bool HotbarExpanded = false;


        private Item _heldStash;
        private int _heldIndex = -1;
        private bool _heldSwappedThisTick;
        private bool InAttackMode => HotbarExpanded || (Animator?.IsBusy ?? false);

        //PLAYER ANIMATION HANDLING
        public BendingAnimator Animator = new BendingAnimator();
        private readonly List<(int ticks, Action action)> _timers = new();
        public void RunAfter(int ticks, Action action) => _timers.Add((ticks, action));
        private readonly Dictionary<string, int> _moveCooldown = new();

        //ATTACK STUFF
        public struct AttackUpgrades
        {
            public float MeleeDamageMult;
            public float ProjectileDamageMult;
            public float MovementDamageMult;
            public float DashIFramesBonus;
            public float CooldownMult;
            public float CostMult;
        }

        //DAMAGE CLASS STUFF
        public StatModifier FireTotalDamage => Player.GetTotalDamage<FireDamageClass>();
        public float FireTotalCrit => Player.GetTotalCritChance<FireDamageClass>();
        public StatModifier FireTotalKnockback => Player.GetTotalKnockback<FireDamageClass>();


        public StatModifier WaterTotalDamage => Player.GetTotalDamage<WaterDamageClass>();
        public float WaterTotalCrit => Player.GetTotalCritChance<WaterDamageClass>();
        public StatModifier WaterTotalKnockback => Player.GetTotalKnockback<WaterDamageClass>();

        public StatModifier EarthTotalDamage => Player.GetTotalDamage<EarthDamageClass>();
        public float EarthTotalCrit => Player.GetTotalCritChance<EarthDamageClass>();
        public StatModifier EarthTotalKnockback => Player.GetTotalKnockback<EarthDamageClass>();


        public StatModifier AirTotalDamage => Player.GetTotalDamage<AirDamageClass>();
        public float AirTotalCrit => Player.GetTotalCritChance<AirDamageClass>();
        public StatModifier AirTotalKnockback => Player.GetTotalKnockback<AirDamageClass>();



        public AttackUpgrades Up = new AttackUpgrades
        {
            MeleeDamageMult = 1f,
            ProjectileDamageMult = 1f,
            MovementDamageMult = 1f,
            DashIFramesBonus = 0f,
            CooldownMult = 1f,
            CostMult = 1f,
        };

        public float GetDamageMult(AttackTags tags)
        {
            float m = 1f;
            if ((tags & AttackTags.Melee) != 0) m *= Up.MeleeDamageMult;
            if ((tags & AttackTags.Projectile) != 0) m *= Up.ProjectileDamageMult;

            return m;
        }

        public float GetCooldownMult(AttackTags tags) => Up.CooldownMult;
        public float GetCostMult(AttackTags tags) => Up.CostMult;

        public override void Initialize()
        {
            hasChosenBending = false;
            breatheTimer = 60;

            //initializing slots once per player
            for (int i = 0; i < MoveSlots.Length; i++) MoveSlots[i].Unlocked = i < UnlockedSlots;

            // adding fireFist(testMove) to test with hotbar
            if (MoveRegistry.Get("fire_fist") != null)
            {
                MoveSlots[0].MoveId = "fire_fist";
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["chosenStyle"] = (int)chosenStyle;
            tag["hasLearnedFire"] = hasLearnedFire;
            tag["hasLearnedWater"] = hasLearnedWater;
            tag["hasLearnedEarth"] = hasLearnedEarth;
            tag["hasLearnedAir"] = hasLearnedAir;

            tag["breath"] = breath;
            tag["maxBreath"] = maxBreath; 
            
            tag["UnlockedSlots"] = UnlockedSlots;
            tag["SelectedSlotIndex"] = SelectedSlotIndex;
            tag["HotbarExpanded"] = HotbarExpanded;

            var slotIds = new string[6];
            var slotUnlocks = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                slotIds[i] = MoveSlots[i].MoveId ?? "";
                slotUnlocks[i] = MoveSlots[i].Unlocked;
            }

            tag["MoveSlotIds"] = slotIds;
            tag["MoveSlotUnlocked"] = slotUnlocks;
        }

        public override void LoadData(TagCompound tag)
        {
            chosenStyle = (BendingStyle)(int)tag.GetInt("chosenStyle");

            hasLearnedFire = tag.GetBool("hasLearnedFire");
            hasLearnedWater = tag.GetBool("hasLearnedWater");
            hasLearnedEarth = tag.GetBool("hasLearnedEarth");
            hasLearnedAir = tag.GetBool("hasLearnedAir");

            breath = tag.GetFloat("breath");
            breatheTimer = tag.GetInt("breatheTimer");

            breatheTimer = 60;
            breatheCooldownTimer = 0;
            isActivelyBreathing = false;

            if (chosenStyle != BendingStyle.None)
            {
                hasChosenBending = true;
            }
            else
            {
                hasChosenBending = false;
            }

            UnlockedSlots = tag.GetInt("UnlockedSlots");
            SelectedSlotIndex = tag.GetInt("SelectedSlotIndex");
            HotbarExpanded = tag.GetBool("HotbarExpanded");

            var slotIds = tag.Get<string[]>("MoveSlotIds");
            var slotUnlocks = tag.Get<bool[]>("MoveSlotUnlocked");
            for (int i = 0; i < 6; i++)
            {
                MoveSlots[i].Unlocked = (i < UnlockedSlots) || (slotUnlocks != null && i < slotUnlocks.Length && slotUnlocks[i]);
                MoveSlots[i].MoveId = (slotIds != null && i < slotIds.Length) ? slotIds[i] : "";
            }

        }

        public override void PostUpdate()
        {            
            if (hasLearnedFire)
            {
                if (!Player.HasBuff(ModContent.BuffType<firebenderBuff>()))
                {
                    Player.AddBuff(ModContent.BuffType<firebenderBuff>(), 2);
                }

                HandleBreathRegeneration();
                HandleBreatheCooldown();
            }

            if (hasLearnedWater)
            {
                if (!Player.HasBuff(ModContent.BuffType<waterbenderBuff>()))
                {
                    Player.AddBuff(ModContent.BuffType<waterbenderBuff>(), 2);
                }
            }

            if (hasLearnedEarth)
            {
                if (!Player.HasBuff(ModContent.BuffType<earthbenderBuff>()))
                {
                    Player.AddBuff(ModContent.BuffType<earthbenderBuff>(), 2);
                }
            }

            if (hasLearnedAir)
            {
                if (!Player.HasBuff(ModContent.BuffType<airbenderBuff>()))
                {
                    Player.AddBuff(ModContent.BuffType<airbenderBuff>(), 2);
                }
            }

            //attack timing control
            for (int i = _timers.Count - 1; i >= 0; --i)
            {
                var (t, a) = _timers[i];
                t--;
                if (t <= 0) { a?.Invoke(); _timers.RemoveAt(i); }
                else _timers[i] = (t, a);
            }

            Animator.Update(Player);


            //attack handling 
            if (InAttackMode)
            {
                Player.noItems = true;
                Player.noBuilding = true;
                Player.controlUseItem = false;
                Player.releaseUseItem = true;
                Player.controlUseTile = false;
                Player.cursorItemIconEnabled = false;
                Player.cursorItemIconID = 0;
                Player.cursorItemIconText = null;
            }
        }

        private void HandleBreathRegeneration()
        {
            //breathregen constants (mightchange)
            const int REGEN_DELAY_TICKS = 180; // 3 second regen delay
            const float REGEN_RATE = 0.08f; //default regen rate
            const float REGEN_FAST_RATE = 0.4f; //when breathing, regenfaster

            if (takenBreath)
            {
                breathRegenTimer = 0;
                takenBreath = false;
            }

            if (breath < maxBreath)
            {
                breathRegenTimer++;

                //checking if we're breathing
                bool activelyBreathing = ATLAMod.BreatheKeybind.Current;

                //can only breathe if timer is at 60 and not in cooldown
                if (activelyBreathing && breatheTimer >= 0 && breatheCooldownTimer == 0)
                {
                    if (!isActivelyBreathing)
                    {
                        isActivelyBreathing = true;
                        activeBreathingDuration = 0;
                        //indication of breathing - changing this to visual meter effects later
                    }

                    //continuing breathing and handling timers
                    breath = Math.Min(maxBreath, breath + REGEN_FAST_RATE);
                    breatheTimer--;
                    activeBreathingDuration++;
                    
                    //timer reaches 0 and starts cooldown
                    if (breatheTimer <= 0)
                    {
                        StopActiveBreathing();
                    }
                }
                else if (isActivelyBreathing) //player released key or ran out of time
                {                    
                    StopActiveBreathing();
                }
                else if(breathRegenTimer >= REGEN_DELAY_TICKS) //passive regen - slow rate after delay
                {
                    breath = Math.Min(maxBreath, breath + REGEN_RATE);
                }
            }
            else
            {
                breathRegenTimer = 0;
                if (isActivelyBreathing)
                {
                    StopActiveBreathing();
                }
            }
        }

        private void StopActiveBreathing()
        {
            if (isActivelyBreathing)
            {
                isActivelyBreathing = false;

                breatheCooldownTimer = 300; //5 seconds

                Player.AddBuff(ModContent.BuffType<BreathExhaustion>(), breatheCooldownTimer); //ADD BUFF
            }
        }

        private void HandleBreatheCooldown()
        {
            if (breatheCooldownTimer > 0)
            {
                breatheCooldownTimer--;

                if(breatheCooldownTimer == 0)
                {
                    breatheTimer = 60;
                    //CHANGE THIS - maybe a sound or visual or something.
                }
            }
        }
        public void ConsumeBreath(float amount)
        {
            if (breath >= amount)
            {
                breath = Math.Max(0, breath - amount);
                takenBreath = true;
            }
        }

        public bool HasEnoughBreath(float amount)
        {
            return breath >= amount;
        }

        // this is to check if breath CAN be consumed
        public bool TryConsumeBreath(float amount)
        {
            if (HasEnoughBreath(amount))
            {
                ConsumeBreath(amount);
                return true;
            }

            if (breathShakeTicks == 0) breathShakeTicks = 12;
            if (breathFailSoundCooldown == 0)
            {
                breathFailSoundCooldown = 12;
                SoundEngine.PlaySound(Terraria.ID.SoundID.Item42 with { Volume = 0.6f, Pitch = -0.5f });
            }
            return false;
        }

        //HANDLING ATTACKHOTBAR STUFF
        public override void ResetEffects()
        {
            var keys = new List<string>(_moveCooldown.Keys);
            foreach (var k in keys)
            {
                _moveCooldown[k] = System.Math.Max(0, _moveCooldown[k] - 1);
                if (_moveCooldown[k] == 0) _moveCooldown.Remove(k);
            }

            if (breathShakeTicks > 0) breathShakeTicks--;
            if (breathFailSoundCooldown > 0) breathFailSoundCooldown--;
        }

        public bool IsMoveOnCooldown(string id) => _moveCooldown.ContainsKey(id);
        public void SetMoveCooldown(string id, int ticks)
        {
            if (ticks <= 0) return;
            _moveCooldown[id] = ticks;
        }

        public bool AssignMoveToSlot(int slot, string moveId)
        {
            if (slot < 0 || slot >= MoveSlots.Length) return false;
            if (!MoveSlots[slot].Unlocked) return false;
            MoveSlots[slot].MoveId = moveId;
            return true;
        }

        public void SelectSlot(int slot)
        {
            if (slot >= 0 && slot  < MoveSlots.Length && MoveSlots[slot].Unlocked)
            {
                SelectedSlotIndex = slot;
            }
        }

        public bool TryUseSelectedMove()
        {
            var slot = MoveSlots[SelectedSlotIndex];
            if (string.IsNullOrEmpty(slot.MoveId)) { Main.NewText("[ATLA] No move in selected slot"); return false; }
           
            var move = Bending.MoveRegistry.Get(slot.MoveId);
            if (move == null) { Main.NewText($"[ATLA] Move not found: {slot.MoveId}"); return false; }

            if (!move.CanUse(Player, this)) return false;

            move.OnUse(Player, this);
            Main.NewText($"[ATLA] Cast {move.Name}");

            return true;
        }        

        public override void ProcessTriggers(TriggersSet triggerSet)
        {
            if (!hasLearnedFire)
            {
                return;
            }

            //TESTING FOR USING BREATH
            if (ATLAMod.UseBreathKeyBind.JustPressed)
            {
                float breathCost = 5f;

                if (TryConsumeBreath(breathCost))
                {
                    Main.NewText($"BREATHUSED - " + breath);
                }
                else
                {
                    Main.NewText("NOT ENOUH BREATH");
                }
            }
            
            if (chosenStyle == BendingPlayer.BendingStyle.None)
            {
                if (ATLAMod.ToggleAttackHotbar != null && ATLAMod.ToggleAttackHotbar.JustPressed)
                {
                    HotbarExpanded = false;
                    Main.NewText("Choose a bending style using your Bending Scroll to access your Moveset.");
                }
                return;
            }

            if (ATLAMod.ToggleAttackHotbar != null && ATLAMod.ToggleAttackHotbar.JustPressed)
            {
                HotbarExpanded = !HotbarExpanded;
            }

            if (!HotbarExpanded)
            {
                return;
            }

            if (HotbarExpanded)
            {
                if (PlayerInput.Triggers.JustPressed.Hotbar1) { SelectSlot(0); PlayerInput.Triggers.Current.Hotbar1 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar2) { SelectSlot(1); PlayerInput.Triggers.Current.Hotbar2 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar3) { SelectSlot(2); PlayerInput.Triggers.Current.Hotbar3 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar4) { SelectSlot(3); PlayerInput.Triggers.Current.Hotbar4 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar5) { SelectSlot(4); PlayerInput.Triggers.Current.Hotbar5 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar6) { SelectSlot(5); PlayerInput.Triggers.Current.Hotbar6 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar7) { SelectSlot(5); PlayerInput.Triggers.Current.Hotbar7 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar8) { SelectSlot(5); PlayerInput.Triggers.Current.Hotbar8 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar9) { SelectSlot(5); PlayerInput.Triggers.Current.Hotbar9 = false; }
                if (PlayerInput.Triggers.JustPressed.Hotbar10) { SelectSlot(5); PlayerInput.Triggers.Current.Hotbar10 = false; }

                Player.noItems = true;
                Player.controlUseItem = false;
                Player.releaseUseItem = true;
                Player.itemAnimation = 0;
                Player.itemTime = 0;

                Player.controlUseTile = false;
                Player.noBuilding = true;

                Player.cursorItemIconEnabled = false;
                Player.cursorItemIconID = 0;
                Player.cursorItemIconText = null;
            }

            bool overUI = Main.LocalPlayer.mouseInterface;
            if(!overUI && PlayerInput.Triggers.JustPressed.MouseLeft)
            {
                bool cast = TryUseSelectedMove();                

                PlayerInput.Triggers.Current.MouseLeft = false;
                Player.controlUseItem = false;
                Player.releaseUseItem = false;
                Player.itemAnimation = 0;
                Player.itemTime = 0;
                Player.controlUseTile = false;
                Player.controlThrow = false;
                Player.noItems = true;
            }

            if (!overUI && PlayerInput.Triggers.Current.MouseLeft)
            {
                PlayerInput.Triggers.Current.MouseLeft = false;
                Player.controlUseItem = false;
                Player.controlUseTile = false;
                Player.controlThrow = false;
                Player.noItems = true;                
            }
        }

        public override bool PreItemCheck()
        {            

            if (Player.whoAmI == Main.myPlayer && InAttackMode)
            {
                if (!_heldSwappedThisTick)
                {
                    _heldIndex = Player.selectedItem;
                    _heldStash = Player.inventory[Player.selectedItem].Clone();
                    Player.inventory[Player.selectedItem].TurnToAir();
                    _heldSwappedThisTick = true;
                }

                Player.noItems = true;
                Player.noBuilding = true;
                Player.controlUseItem = false;
                Player.releaseUseItem = true;
                Player.controlUseTile = false;
                Player.itemAnimation = 0;
                Player.itemTime = 0;
                Player.cursorItemIconEnabled = false;
                Player.cursorItemIconID = 0;
                Player.cursorItemIconText = null;
            }

            return true;
        }

        public override void PostItemCheck()
        {
            if (_heldSwappedThisTick)
            {
                if (Player.inventory[Player.selectedItem].IsAir)
                    Player.inventory[Player.selectedItem] = _heldStash;
                else
                {
                    int idx = Array.FindIndex(Player.inventory, it => it.IsAir);
                    if (idx >= 0) Player.inventory[idx] = _heldStash;
                    else Player.QuickSpawnItem(Player.GetSource_Misc("BendingRestore"), _heldStash);
                }

                _heldStash = null;
                _heldSwappedThisTick = false;
            }
        }
    }
}
