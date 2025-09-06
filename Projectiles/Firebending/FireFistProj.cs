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
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 0;

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
            // small impact puff + sound
            ImpactBurst();
            return true; // kill on hit
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // on-hit burst for feedback
            ImpactBurst();
            Projectile.Kill();
        }       

        private void ImpactBurst()
        {                        

            Vector2 pos = Projectile.Center;

            // Impact normal: use incoming velocity; fallback to facing dir
            Vector2 normal = Projectile.oldVelocity;
            if (normal.LengthSquared() < 0.0001f) normal = new Vector2(Projectile.spriteDirection, 0f);
            normal.Normalize();

            FireBurstEffects.SpawnBurst(pos, normal, FireBurstEffects.BurstSize.Small, Projectile.owner, baseDamage: 0);

            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.85f, PitchVariance = 0.15f }, pos);            
            Lighting.AddLight(pos, 1.2f, 0.6f, 0.15f);

            // --- OPTIONAL: tiny hit-stop + micro screen shake for local player ---
            if (Main.myPlayer == Projectile.owner)
            {
                // Hit-stop feel: briefly slow the local player (1–2 ticks)
                Player lp = Main.LocalPlayer;
                lp.velocity *= 0.92f;

                // Micro “shake”: nudge the screen once (kept subtle)
                Main.screenPosition += Main.rand.NextVector2Circular(1.5f, 1.5f);
            }

            // --- DUST: EMBER RING + SPARKS ---
            // Size scales a bit with projectile speed so faster hits pop harder
            float speedMag = MathHelper.Clamp(Projectile.oldVelocity.Length(), 4f, 18f);
            int emberCount = (int)MathHelper.Lerp(10, 20, (speedMag - 4f) / 14f);

            for (int i = 0; i < emberCount; i++)
            {
                // Wide cone around the impact normal
                Vector2 dir = normal.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-85f, 85f)));
                float spd = Main.rand.NextFloat(1.2f, 3.8f) * (speedMag / 12f);

                int d = Dust.NewDust(pos - new Vector2(8f), 16, 16,
                                     ModContent.DustType<EmberDust>(),
                                     dir.X * spd, dir.Y * spd,
                                     0, default,
                                     Main.rand.NextFloat(1.0f, 1.35f));

                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = Main.rand.NextFloat(0.25f, 0.55f);
                Main.dust[d].alpha = 0;
            }

            // --- DUST: A FEW THICKER TORCH FLARES FOR BODY ---
            for (int i = 0; i < emberCount / 3; i++)
            {
                Vector2 dir = normal.RotatedByRandom(MathHelper.ToRadians(55));
                float spd = Main.rand.NextFloat(0.6f, 1.8f) * (speedMag / 12f);

                int d = Dust.NewDust(pos - new Vector2(10f), 20, 20,
                                     Terraria.ID.DustID.Torch,
                                     dir.X * spd, dir.Y * spd,
                                     100, default,
                                     Main.rand.NextFloat(1.25f, 1.65f));
                Main.dust[d].noGravity = true;
            }

            // --- DUST: A FEW SMOKE WISPS FOR DECAY ---
            for (int i = 0; i < emberCount / 4; i++)
            {
                Vector2 dir = normal.RotatedByRandom(MathHelper.ToRadians(45));
                float spd = Main.rand.NextFloat(0.4f, 1.2f);

                int d = Dust.NewDust(pos - new Vector2(12f), 24, 24,
                                     Terraria.ID.DustID.Smoke,
                                     dir.X * spd, dir.Y * spd,
                                     140, default,
                                     Main.rand.NextFloat(1.0f, 1.4f));
                Main.dust[d].noGravity = false;
            }
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
