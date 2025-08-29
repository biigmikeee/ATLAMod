using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ATLAMod.Projectiles
{
    public class FireFistProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 40;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // one-time spawn burst + cast sound
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;

                SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.8f }, Projectile.Center); // magic/flame cast sound

                // small initial fiery puff so you see it spawned
                for (int i = 0; i < 8; i++)
                {
                    var v = Main.rand.NextVector2Circular(2.2f, 2.2f);
                    int d = Dust.NewDust(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.Torch, v.X, v.Y, 100, default, 1.2f);
                    Main.dust[d].noGravity = true;
                }
            }

            // face travel direction (works fine even with no real sprite)
            Projectile.rotation = Projectile.velocity.ToRotation();

            // add a warm light (R,G,B)
            Lighting.AddLight(Projectile.Center, 1.2f, 0.6f, 0.2f);

            // main flame dust (a couple per tick)
            int dust1 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 160, default, 1.1f);
            Main.dust[dust1].noGravity = true;
            Main.dust[dust1].velocity = Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(1.0f, 1.0f);

            if (Main.rand.NextBool(3))
            { // occasional hotter spark
                int dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Flare, 0f, 0f, 120, default, 1.0f);
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].velocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.6f, 1.6f);
            }

            // draw a faint trail using old positions (convert each old pos into a tiny ember)
            if (Projectile.oldPos != null)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f;
                    float scale = MathHelper.Lerp(0.9f, 0.4f, i / (float)Projectile.oldPos.Length);
                    int td = Dust.NewDust(trailPos - new Vector2(2, 2), 4, 4, DustID.Torch, 0f, 0f, 180, default, scale);
                    Main.dust[td].noGravity = true;
                    Main.dust[td].velocity *= 0.1f;
                }
            }

            // optional: tiny velocity damp to look punchy (comment out if you want full straight speed)
            // Projectile.velocity *= 0.995f;
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

        public override void OnKill(int timeLeft)
        {
            // death puff (in case time runs out)
            ImpactBurst();
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f }, Projectile.Center); // soft explosion-ish pop
        }

        private void ImpactBurst()
        {
            for (int i = 0; i < 12; i++)
            {
                var v = Main.rand.NextVector2Circular(2.8f, 2.8f);
                int d = Dust.NewDust(Projectile.Center - new Vector2(6, 6), 12, 12, DustID.Torch, v.X, v.Y, 100, default, 1.3f);
                Main.dust[d].noGravity = true;
            }
        }
    }
}
