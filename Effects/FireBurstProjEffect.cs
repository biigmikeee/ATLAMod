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
            Projectile.timeLeft = FrameCount * TicksPerFrame + 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.hide = false;
            
        }

        public override void AI()
        {                        

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
            
            Lighting.AddLight(Projectile.Center, 1.0f * Projectile.scale, 0.5f * Projectile.scale, 0.12f * Projectile.scale);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int srcX = 0;
            int srcY = VerticalStride * Projectile.frame;
            Rectangle source = new Rectangle(srcX, srcY, FrameW, FrameH);
            
            Vector2 origin = new Vector2(FrameW / 2f, FrameH / 2f);

            Vector2 along = new Vector2((float)Math.Cos(Projectile.rotation), (float)Math.Sin(Projectile.rotation));

            float inset = Projectile.ai[0];
            float lateral = Projectile.ai[1];

            Vector2 drawPos = Projectile.Center - along * inset - Main.screenPosition;

            Vector2 drawWorld = Projectile.Center - along * inset + new Vector2(Projectile.spriteDirection * lateral, 0f);

            SpriteEffects fx = SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(
                tex,
                drawWorld - Main.screenPosition,
                source,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                fx,
                0
            );
            return false;
        }
    }
}
