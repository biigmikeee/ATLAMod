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

        public override void Initialize()
        {
            hasChosenBending = false;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["chosenStyle"] = (int)chosenStyle;
            tag["hasLearnedFire"] = hasLearnedFire;
            tag["hasLearnedWater"] = hasLearnedWater;
            tag["hasLearnedEarth"] = hasLearnedEarth;
            tag["hasLearnedAir"] = hasLearnedAir;
        }

        public override void LoadData(TagCompound tag)
        {
            chosenStyle = (BendingStyle)(int)tag.GetInt("chosenStyle");

            hasLearnedFire = tag.GetBool("hasLearnedFire");
            hasLearnedWater = tag.GetBool("hasLearnedWater");
            hasLearnedEarth = tag.GetBool("hasLearnedEarth");
            hasLearnedAir = tag.GetBool("hasLearnedAir");
        }

        public override void PostUpdate()
        {            
            if (hasLearnedFire)
            {
                if (!Player.HasBuff(ModContent.BuffType<firebenderBuff>()))
                {
                    Player.AddBuff(ModContent.BuffType<firebenderBuff>(), 2);
                }
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

            if (chosenStyle != BendingStyle.Fire)
            {
                return;
            }
        }

        public void ConsumeBreath(float amount)
        {
            breath = Math.Max(0, breath - amount);
        }

        public bool HasEnoughBreath(float amount)
        {
            return breath >= amount;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (ATLAMod.UseBreathKeyBind.JustPressed)
            {
                ConsumeBreath(0.1f);
                Main.NewText("BREATHUSED - " + breath);
            }

            if (ATLAMod.BreatheKeybind.JustPressed)
            {               
                breath = Math.Min(1f, breath + 0.25f);
                Main.NewText("TOOKBREATH - " + breath);
            }
        }
    }
}
