using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace ATLAMod.Dusts.Fire
{
    public class EmberDust : ModDust
    {
        public override string Texture => "ATLAMod/Dusts/Fire/EmberDust";
        // Path to your 10x36 spritesheet

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale = 1f;
            dust.frame = new Rectangle(0, 0, 10, 10); // start at frame 0
        }

        public override bool Update(Dust dust)
        {
            return false;
        }

        public override bool PreDraw(Dust dust)
        {


            return false; // don’t let vanilla draw
        }
    }
}
