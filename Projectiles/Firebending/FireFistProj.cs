using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ATLAMod.Dusts.Fire;
using Terraria.DataStructures;
using ATLAMod.Effects;
using ATLAMod.Systems.Bending;

namespace ATLAMod.Projectiles.Firebending
{
    public class FireFistProj : ModProjectile
    {

        public override string Texture => "ATLAMod/Projectiles/Firebending/fireFistProj";

        private const int FrameWidth = 60;
        private const int FrameHeight = 12;
        private const int TotalFrames = 12;
        private const int TicksPerFrame = 2;
        private const int VerticalStride = FrameHeight + 2;

        private bool _burstDone;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = TotalFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width = FrameWidth;
            Projectile.height = FrameHeight;
            Projectile.friendly = true;
            Projectile.penetrate = 1;            
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = ModContent.GetInstance<FireDamageClass>();

        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.7f, Pitch = 0.1f }, Projectile.Center);
        }

        public override void AI()
        {
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation();

            Lighting.AddLight(Projectile.Center, 0.85f, 0.65f, 0.15f);

            Projectile.frameCounter++;
            if(Projectile.frameCounter >= TicksPerFrame)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if(Projectile.frame >= TotalFrames)
                {
                    Projectile.Kill();
                }
            }

            Projectile.localAI[0]++;
            if (Projectile.localAI[0] >= 4 && Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(Projectile.Center - new Vector2(5, 5), 10, 10, ModContent.DustType<EmberDust>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.95f, 1.1f));

                Vector2 tangent = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                float speed = Main.rand.NextFloat(0.3f, 0.6f);
                d.velocity = tangent * (Main.rand.NextBool() ? -speed : speed) + Main.rand.NextVector2Circular(0.2f, 0.2f);
                d.alpha = 0;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {            
            ImpactBurst();
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // on-hit burst for feedback
            ImpactBurst();
            Projectile.Kill();
        }

        private void ImpactBurst()
        {
            if (_burstDone) return;
            _burstDone = true;

            var src = Projectile.GetSource_Death();

            Vector2 normal = Projectile.oldVelocity;
            if (normal.LengthSquared() < 0.001f) normal = Projectile.velocity;
            if (normal.LengthSquared() < 0.001f) normal = new Vector2(Projectile.spriteDirection, 0f);
            normal.Normalize();

            float spriteHalf = 60f * 0.5f - 1f;

            float aabbHalf = Math.Abs(normal.X) * (Projectile.width * 0.5f - 1f) +
                Math.Abs(normal.Y) * (Projectile.height * 0.5f - 1f);

            float noseDist = Math.Min(spriteHalf, Math.Max(0f, aabbHalf));

            Vector2 hitPos = Projectile.Center + normal * noseDist;

            FireBurstEffects.SpawnBurst(src, hitPos, normal, FireBurstEffects.BurstSize.Small, Projectile.owner);
        }
       
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            int srcX = 0;
            int srcY = VerticalStride * Projectile.frame;
            Rectangle source = new Rectangle(srcX, srcY, FrameWidth, FrameHeight);

            Vector2 origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                source,
                lightColor,
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
