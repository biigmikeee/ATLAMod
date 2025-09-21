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

        private const int FrameW = 10;
        private const int FrameH = 10;
        private const int StrideY = FrameH + 2;
        private const int FrameCount = 3;
        public override string Texture => "ATLAMod/Dusts/Fire/EmberDust";

        private class DustState
        {
            public int FrameTimer;
            public int FrameIndex;
            public int TicksPerFrame;
            public int SpinDir;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale = Main.rand.NextFloat(0.95f, 1.35f);
            dust.rotation = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);

            if (dust.velocity.LengthSquared() < 0.001f)
            {
                dust.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
            }

            var state = new DustState
            {
                FrameTimer = 0,
                FrameIndex = Main.rand.NextBool() ? 0 : 1,
                TicksPerFrame = 6,
                SpinDir = Main.rand.NextBool() ? 1 : -1
            };
            dust.customData = state;

            dust.frame = new Rectangle(0, 0, FrameW, FrameH);
        }
        public override bool Update(Dust dust)
        {
            if (dust.customData is DustState state)
            {
                state.FrameTimer++;
                if (state.FrameTimer >= state.TicksPerFrame)
                {
                    state.FrameTimer = 0;
                    state.FrameIndex++;

                    if (state.FrameIndex >= FrameCount)
                    {
                        dust.active = false;
                        return false;
                    }

                    dust.frame.Y = state.FrameIndex * StrideY;
                }

                dust.rotation *= 0.04f * state.SpinDir;
            }

            dust.position += dust.velocity;
            dust.velocity *= 0.965f;
            dust.velocity.Y -= 0.02f;

            Lighting.AddLight(dust.position, 0.9f * dust.scale, 0.5f * dust.scale, 0.08f * dust.scale);
            dust.scale *= 0.985f;
            dust.alpha = (int)MathHelper.Clamp(255f * (1.25f - dust.scale), 0f, 160f);

            if (dust.scale < 0.45f)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            return true;
        }
    }
}
