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

namespace ATLAMod.Systems.Bending.Moves.Fire
{
    public class FireFist : BendingMove
    {
        public FireFist() : base(
            id: "fire_fist",
            name: "Fire Fist",
            style: BendingStyle.Fire,
            iconPath: "ATLAMod/Assets/Moves/FireFistHotbar_iconTEST",
            cost: 10,
            cdTicks: 30)
        { }

        public override void OnUse(Player p, BendingPlayer bp)
        {
            base.OnUse(p, bp);

            if (p.whoAmI == Main.myPlayer)
            {
                var speed = p.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX) * 12f;
                int projType = ModContent.ProjectileType<Projectiles.FireFistProj>();
                Projectile.NewProjectile(
                    spawnSource: p.GetSource_FromThis(),
                    position: p.Center + speed.SafeNormalize(Vector2.UnitX) * 16f,
                    velocity: speed,
                    Type: projType, Damage: 30, KnockBack: 3f, Owner: p.whoAmI);
            }
        }
    }
}
