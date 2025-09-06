using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ATLAMod.Projectiles.Firebending;
using Terraria.DataStructures;

namespace ATLAMod.Effects
{
    public static class FireBurstEffects
    {
        public enum BurstSize { Small, Medium, Large }

        public static void SpawnBurst (Vector2 pos, Vector2 normal, BurstSize size, int owner, int baseDamage = 0)
        {
            float scale; int frames = 8; int dustCount; float emberSpeed;
            switch (size)
            {
                case BurstSize.Small: scale = 0.7f; dustCount = 8; emberSpeed = 3.0f; break;
                case BurstSize.Medium: scale = 0.1f; dustCount = 14; emberSpeed = 3.8f; break;
                default: scale = 1.3f; dustCount = 20; emberSpeed = 4.6f; break;
            }

            int type = ModContent.ProjectileType<FireBurstProjEffect>();
            int who = Main.myPlayer; // purely visual; owner can be spawner
            float rot = normal.ToRotation();
            int proj = Projectile.NewProjectile(
                spawnSource: new EntitySource_Misc("FireBurstEffects"),
                position: pos,
                velocity: Vector2.Zero,
                Type: type,
                Damage: baseDamage, // usually 0 (visual); could add tiny AOE if desired
                KnockBack: 0f,
                Owner: owner >= 0 ? owner : who,
                ai0: rot,          // pass rotation through ai
                ai1: scale         // pass scale through ai
            );

            // Sound (swap to your custom SoundStyle if you have one)
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.8f, PitchVariance = 0.15f }, pos);

            // Light
            Lighting.AddLight(pos, 1.4f * scale, 0.8f * scale, 0.25f * scale);

            // Accent dust: ember ring + few sparks
            // Replace DustID.Torch with your ModContent.DustType<...> when ready
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dir = (normal.RotatedByRandom(MathHelper.ToRadians(85))) * Main.rand.NextFloat(0.6f, 1.0f);
                Vector2 v = dir.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.5f, emberSpeed) * scale;

                int d = Dust.NewDust(pos - new Vector2(8), 16, 16, DustID.Torch, v.X, v.Y, 100, default, Main.rand.NextFloat(1.1f, 1.5f) * scale);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = Main.rand.NextFloat(0.3f, 0.6f);
            }

            // A few heavier, short-lived smoke wisps (optional)
            for (int i = 0; i < dustCount / 4; i++)
            {
                Vector2 v = normal.RotatedByRandom(MathHelper.ToRadians(60)) * Main.rand.NextFloat(0.5f, 2.0f) * scale;
                int d = Dust.NewDust(pos - new Vector2(8), 16, 16, DustID.Smoke, v.X, v.Y, 150, default, Main.rand.NextFloat(1.1f, 1.6f) * scale);
                Main.dust[d].noGravity = false;
            }
        }
    }
}
