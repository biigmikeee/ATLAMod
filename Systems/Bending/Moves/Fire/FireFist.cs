using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLAMod.Systems.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using ATLAMod.Projectiles.Firebending;

namespace ATLAMod.Systems.Bending.Moves.Fire
{
    public class FireFist : BendingMove
    {
        public FireFist() : base(
            id: "fire_fist",
            name: "Fire Fist",
            style: BendingPlayer.BendingStyle.Fire,
            iconPath: "ATLAMod/Assets/UITextures/attackHotbarTest/firefistIconTest",
            cost: 10,
            cdTicks: 30)
        { }

        public override void OnUse(Player p, BendingPlayer bp)
        {
            base.OnUse(p, bp);
            // inside FireFist.OnUse(Player p, BendingPlayer bp)
            if (p.whoAmI != Main.myPlayer) return;

            // face the cursor
            Vector2 aim = p.DirectionTo(Main.MouseWorld);
            if (aim.LengthSquared() < 0.001f) aim = new Vector2(p.direction, 0f);
            aim.Normalize();

            int type = ModContent.ProjectileType<FireFistProj>();
            Vector2 spawn = p.Center + aim * 20f;
            Vector2 vel = aim * 12f;

            Projectile.NewProjectile(p.GetSource_FromThis(), spawn, vel, type, 30, 3f, p.whoAmI);

            // --- brief wand-like pose (front arm points toward cursor) ---
            p.direction = (Main.MouseWorld.X >= p.Center.X) ? 1 : -1;

            // composite arm uses rotation relative to "down"; subtract PiOver2 to point forward
            float armRot = aim.ToRotation() - MathHelper.PiOver2;
            p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);

            // tiny item animation window so vanilla pose shows (even though no item is used)
            p.itemAnimation = p.itemAnimationMax = 14;
            p.itemTime = 14;
        }
    }
}
