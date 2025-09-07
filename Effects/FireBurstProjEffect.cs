using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ATLAMod.Effects
{
    public class FireBurstProjEffect : ModProjectile
    {
        private const int FrameW = 48;
        private const int FrameH = 52;
        private const int FrameCount = 8;
        private const int VerticalStride = FrameH + 2;
        private const int TicksPerFrame = 2; // fast pop

        public override string Texture => "ATLAMod/Effects/fireBurstEffect";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FrameCount;
        }

        public override void SetDefaults()
        {
            Projectile.width = FrameW;
            Projectile.height = FrameH;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60; // safety
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.hide = true; // visual only
        }

        public override void AI()
        {
            // ai[0] = rotation, ai[1] = scale
            Projectile.rotation = Projectile.ai[0];
            Projectile.scale = (Projectile.ai[1] == 0f ? 1f : Projectile.ai[1]);

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= TicksPerFrame)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FrameCount)
                {
                    Projectile.Kill();                    
                }
            }

            // brief bright light
            Lighting.AddLight(Projectile.Center, 1.0f * Projectile.scale, 0.5f * Projectile.scale, 0.12f * Projectile.scale);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int srcX = 0;
            int srcY = VerticalStride * Projectile.frame;
            Rectangle source = new Rectangle(srcX, srcY, FrameW, FrameH);
            
            Vector2 origin = new Vector2(FrameW / 2f, FrameH / 2f);

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                source,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
}
