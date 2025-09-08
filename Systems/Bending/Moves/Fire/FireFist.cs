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
using ATLAMod.Systems.Players.Animation;
using ATLAMod.Effects;

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
            if (p.whoAmI != Main.myPlayer) return;

            // face the cursor
            Vector2 aim = p.DirectionTo(Main.MouseWorld);
            if (aim.LengthSquared() < 0.001f) aim = new Vector2(p.direction, 0f);

            int projType = ModContent.ProjectileType<FireFistProj>();
            Vector2 spawn = p.Center + Vector2.Normalize(aim) * 20f;
            Vector2 vel = Vector2.Normalize(aim) * 8f;
            int dmg = 30; float kb = 3f; int owner = p.whoAmI;

            const int impactDelayTicks = 3;
            const float forward = 8f;
            const float up = 0f;

            //player animation
            bp.Animator.Play(new PunchAction(aim, 7, 5, 8, 4, 7, () =>
            {

                bp.RunAfter(impactDelayTicks, () =>
                {                    
                    Projectile.NewProjectile(p.GetSource_FromThis(), spawn, vel, projType, dmg, kb, owner);
                });
            }));                    
        }
    }
}
