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
        public float maxBreath = 1f;
        public float breath = 1f;
        public bool takenBreath = false;
        public int breathRegenTimer = 0;
        public int breatheTimer = 60;
        public int breatheCooldownTimer = 0;
        public bool isActivelyBreathing = false;
        public int activeBreathingDuration = 0;
        public override void Initialize()
        {
            hasChosenBending = false;
            breatheTimer = 60;
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
        }

        private void HandleBreathRegeneration()
        {
            //breathregen constants (mightchange)
            const int REGEN_DELAY_TICKS = 180; // 3 second regen delay
            const float REGEN_RATE = 0.0008f; //default regen rate
            const float REGEN_FAST_RATE = 0.004f; //when breathing, regenfaster

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
            return false;
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
                float breathCost = 0.1f;

                if (TryConsumeBreath(breathCost))
                {
                    Main.NewText($"BREATHUSED - " + breath);
                }
                else
                {
                    Main.NewText("NOT ENOUH BREATH");
                }
            }                        
        }
    }
}
