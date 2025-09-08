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

            const int impactDelayTicks = 5;
            const float forward = 50f;
            const float up = 8f;

            //player animation
            bp.Animator.Play(new PunchAction(aim, 7, 5, 8, 4, 7, () =>
            {

                bp.RunAfter(impactDelayTicks, () =>
                {
                    Vector2 basePt = p.RotatedRelativePoint(p.MountedCenter, true);
                    Vector2 spawnCenter = basePt + new Vector2(forward, up).RotatedBy(aim.ToRotation());

                    const int projW = 36, projH = 12;
                    Vector2 spawnTopLeft = spawnCenter - new Vector2(projW * 0.5f, projH * 0.5f);

                    //avoiding wall collision on spawn
                    for (int i = 0; i < 3 && Collision.SolidCollision(spawnTopLeft, projW, projH); i++)
                        spawnTopLeft += aim * 4f;

                    Projectile.NewProjectile(p.GetSource_FromThis(), spawnTopLeft, aim * 8f, projType, dmg, kb, owner);
                });
            }));                    
        }
    }
}
