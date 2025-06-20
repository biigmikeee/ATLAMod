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
        //private bool uiShown = false;

        public override void Initialize()
        {
            hasLearnedAir = true;
            hasLearnedFire = true;
            hasLearnedWater = true;
            hasLearnedEarth = true;
            hasChosenBending = true;
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
        }
    }
}
