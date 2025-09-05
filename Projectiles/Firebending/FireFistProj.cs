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

namespace ATLAMod.Projectiles.Firebending
{
    public class FireFistProj : ModProjectile
    {

        public override string Texture => "ATLAMod/Projectiles/Firebending/fireFistProj";

        private const int FrameWidth = 60;
        private const int FrameHeight = 12;
        private const int TotalFrames = 12;

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
            Projectile.timeLeft = 30;
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

            Lighting.AddLight(Projectile.Center, 1.05f, 0.5f, 0.15f);

            Projectile.frameCounter++;
            if(Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if(Projectile.frame >= TotalFrames)
                {
                    Projectile.frame = TotalFrames - 1;
                }
            }

            Projectile.localAI[0]++;
            if (Projectile.localAI[0] >= 4 && Main.rand.NextBool(2))
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
        }       

        private void ImpactBurst()
        {
            // small burst of your custom embers on impact
            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(
                    Projectile.Center - new Vector2(5, 5), 10, 10,
                    ModContent.DustType<EmberDust>(),
                    0f, 0f, 0, default, Main.rand.NextFloat(0.95f, 1.1f)
                );
                d.velocity = Main.rand.NextVector2Circular(1.8f, 1.8f);
                d.alpha = 0;
            }
        }

        // Draw: pick the correct frame from the vertical sheet and add a soft afterimage trail
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Rectangle frameRect = new Rectangle(0, FrameHeight * Projectile.frame, FrameWidth, FrameHeight);
            Vector2 origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);

            // Afterimage trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float t = i / (float)Projectile.oldPos.Length;      // 0..1
                float alpha = 0.5f * (1f - t);
                Color c = new Color(255, 170, 90) * alpha;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                Main.spriteBatch.Draw(tex, pos, frameRect, c, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            }

            // Main sprite
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(tex, drawPos, frameRect, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            return false; // we've drawn it
        }
    }
}
