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
            iconPath: "ATLAMod/Assets/UITextures/hotbarAttackIcons/Fire/fireFistHotbarIcon",
            cost: 1,
            cdTicks: 30)
        { }

        public override void OnUse(Player p, BendingPlayer bp)
        {
            base.OnUse(p, bp);
            if (p.whoAmI != Main.myPlayer) return;

            // face the cursor
            Vector2 aim = p.DirectionTo(Main.MouseWorld);
            if (aim.LengthSquared() < 0.001f) aim = new Vector2(p.direction, 0f);

            const int impactDelayTicks = 5;            

            //player animation
            bp.Animator.Play(new PunchAction(aim, 7, 5, 8, 4, 7, () =>
            {

                bp.RunAfter(impactDelayTicks, () =>
                {
                    Vector2 along = p.DirectionTo(Main.MouseWorld);
                    if (along.LengthSquared() < 0.0001) along = new Vector2(p.direction, 0f);
                    along.Normalize();

                    Vector2 perp = new Vector2(-along.Y * p.gravDir, along.X * p.gravDir);
                    Vector2 facing = new Vector2(p.direction, 0f);
                    if (Vector2.Dot(perp, facing) < 0f) perp = -perp;

                    const float shoulderForward = 0f;
                    const float shoulderUp = 0f;

                    const float forwardFront = 40f;
                    const float forwardBack = 40f;
                    const float lateral = 0f;

                    Vector2 basePt = p.RotatedRelativePoint(p.MountedCenter, true);
                    Vector2 shoulder = basePt + facing * shoulderForward + new Vector2(0f, shoulderUp * p.gravDir);

                    float facingDot = MathHelper.Clamp(Vector2.Dot(along, facing), -1f, 1f);
                    float forward = MathHelper.Lerp(forwardBack, forwardFront, 0.5f * (facingDot + 1f));

                    Vector2 spawnCenter = shoulder + along * forward + perp * lateral;

                    //close projectile collision fixes
                    const int projW = 60, projH = 12;
                    Vector2 half = new Vector2(projW * 0.5f, projH * 0.5f);

                    if (Collision.SolidCollision(spawnCenter - half, projW, projH))
                    {
                        for (int i = 0; i < 3 && Collision.SolidCollision(spawnCenter - new Vector2(projW * 0.5f, projH * 0.5f), projW, projH); i++)
                            spawnCenter += along * 4f;

                        if (Collision.SolidCollision(spawnCenter - half, projW, projH))
                        {
                            Vector2 lo = basePt;
                            Vector2 hi = spawnCenter;
                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 mid = (lo + hi) * 0.5f;
                                if (Collision.SolidCollision(mid - half, projW, projH)) hi = mid; else lo = mid;
                            }
                            spawnCenter = (lo + hi) * 0.5f;
                        }
                    }                    

                    int type = ModContent.ProjectileType<FireFistProj>();
                    int dmg = 30; float kb = 3f;
                    Vector2 vel = along * 4f;

                    Projectile.NewProjectile(
                        p.GetSource_FromThis(),
                        spawnCenter,
                        vel,
                        type,
                        dmg, kb, p.whoAmI);
                });
            }));                    
        }
    }
}
