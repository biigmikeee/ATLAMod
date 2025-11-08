using ATLAMod.Systems.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework;
using ATLAMod.Projectiles.Firebending;
using Terraria.ModLoader;
using ATLAMod.Systems.Players.Animation;

namespace ATLAMod.Systems.Bending.Moves.Fire
{
    public class FireYoyo : BendingMove
    {
        public FireYoyo() : base(
            id: "fire_yoyo",
            name: "Fire Yoyo",
            style: BendingPlayer.BendingStyle.Fire,
            iconPath: "ATLAMod/Assets/UITextures/hotbarAttackIcons/Fire/fireFistHotbarIcon",
            cost: 10,
            cdTicks: 30)
        { Tags = AttackTags.Projectile; }

        public override void OnUse(Player p, BendingPlayer bp)
        {
            base.OnUse(p, bp);
            if (p.whoAmI != Main.myPlayer) return;

            Microsoft.Xna.Framework.Vector2 aim = p.DirectionTo(Main.MouseWorld);
            if (aim.LengthSquared() < 0.001f)
            {
                aim = new Microsoft.Xna.Framework.Vector2(p.direction, 0f);
            }
            aim.Normalize();

            int active = 0;
            for (int i = 0; i <Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == p.whoAmI && proj.type == ModContent.ProjectileType<FireYoyoProj>())
                {
                    active++;
                }
            }

            if (active >= 3) return;

            bool isLastThrow = (active == 2);

            if (!isLastThrow)
            {
                bp.Animator.Play(new MultiThrowAction(aim, 4, 3, 5, isFinalThrow: false, () => SpawnBoomerang(p, bp, aim)));
            }
            else
            {
                bp.Animator.Play(new MultiThrowAction(aim, 5, 6, 9, isFinalThrow: true, () => SpawnBoomerang(p, bp, aim)));
            }
        }

        private void SpawnBoomerang(Player p, BendingPlayer bp, Microsoft.Xna.Framework.Vector2 aim)
        {
            Microsoft.Xna.Framework.Vector2 spawnCenter = p.MountedCenter + aim * 30f;
            Microsoft.Xna.Framework.Vector2 velocity = aim * 14f;

            int baseDamage = 28;
            float baseKnockback = 3f;

            int damage = (int)p.GetTotalDamage<FireDamageClass>().ApplyTo(baseDamage);
            float knockback = p.GetTotalKnockback<FireDamageClass>().ApplyTo(baseKnockback);

            Projectile.NewProjectile(
                p.GetSource_FromThis(),
                spawnCenter,
                velocity,
                ModContent.ProjectileType<FireYoyoProj>(),
                damage,
                knockback,
                p.whoAmI);
        }
    }
}
