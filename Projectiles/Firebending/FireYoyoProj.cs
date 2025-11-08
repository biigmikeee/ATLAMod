using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using ATLAMod.Dusts.Fire;
using ATLAMod.Systems.Bending;
using Terraria;

namespace ATLAMod.Projectiles.Firebending
{
    public class FireYoyoProj : ModProjectile
    {
        public override string Texture => "ATLAMod/Projectiles/Firebending/fireYoyoProj";

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.DamageType = ModContent.GetInstance<FireDamageClass>();
        }

        private bool returning;
        private const float Speed = 14f;
        private const float ReturnAccel = 0.55f;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Lighting.AddLight(Projectile.Center, 0.9f, 0.4f, 0.1f);
            if (Main.rand.NextBool(4))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<EmberDust>(), Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f);
            }

            Vector2 toPlayer = player.Center - Projectile.Center;
            float distance = toPlayer.Length();

            if (!returning)
            {
                if (Projectile.ai[0]++ > 20f || distance > 400f)
                {
                    returning = true;
                    Projectile.tileCollide = false;
                }
            }
            else
            {
                if (distance < 30f)
                {
                    Projectile.Kill();
                    return;
                }

                toPlayer.Normalize();
                Projectile.velocity = (Projectile.velocity * 20f + toPlayer * ReturnAccel * Speed) / 21f;
            }

            Projectile.rotation += 0.45f * Math.Sign(Projectile.velocity.X);
        }
        
        //NEED IMPACT HANDLING

    }
}
