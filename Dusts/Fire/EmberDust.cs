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
            public bool Forward = true;
            public int TicksPerFrame = 4;
            public int SpinDir;
            public float SpinSpeed;
            public float SizeJitter;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;

            var state = new DustState
            {
                FrameTimer = 0,
                FrameIndex = 0,
                TicksPerFrame = 4,
                SpinDir = Main.rand.NextBool() ? 1 : -1,
                SpinSpeed = Main.rand.NextFloat(0.05f, 0.08f),
                SizeJitter = Main.rand.NextFloat(0.60f, 1f)
            };
            dust.customData = state;

            if (dust.scale <= 0.5f) dust.scale = Main.rand.NextFloat(0.60f, 1f);
            dust.scale *= state.SizeJitter;
            dust.rotation = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);

            if (dust.velocity.LengthSquared() < 0.001f)
            {
                dust.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
            }           

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
                    int max = FrameCount - 1;

                    if (state.Forward)
                    {
                        if (state.FrameIndex < max) state.FrameIndex++;
                        else state.Forward = false;
                    }
                    else
                    {
                        if (state.FrameIndex > 0) state.FrameIndex--;
                        else { dust.active = false; return false; }
                    }

                    dust.frame.Y = state.FrameIndex * StrideY;
                }                    

                dust.rotation += state.SpinSpeed * state.SpinDir;
            }

            dust.position += dust.velocity;
            dust.velocity *= 0.967f;
            dust.velocity.Y -= 0.02f;

            Lighting.AddLight(dust.position, 0.9f * dust.scale, 0.5f * dust.scale, 0.08f * dust.scale);
            dust.scale *= 0.990f;
            dust.alpha = (int)MathHelper.Clamp(255f * (1.08f - dust.scale), 0f, 110f);

            if (dust.scale < 0.3f)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            return true;
        }
    }
}
