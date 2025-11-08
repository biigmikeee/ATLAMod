using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace ATLAMod.Systems.Players.Animation
{
    public sealed class MultiThrowAction : IAnimAction
    {
        private readonly int windup, strike, recover;
        private readonly Vector2 aim;
        private readonly bool finalThrow;
        private readonly System.Action fire;
        private int t;
        private bool fired;

        public MultiThrowAction(Vector2 aimDir, int windupTicks, int strikeTicks, int recoverTicks, bool isFinalThrow, System.Action onFire = null)
        {
            aim = aimDir.LengthSquared() > 0 ? Vector2.Normalize(aimDir) : new Vector2(1, 0);
            windup = windupTicks;
            strike = strikeTicks;
            recover = recoverTicks;
            finalThrow = isFinalThrow;
            fire = onFire;
        }

        public bool Update(Player p)
        {
            p.direction = (aim.X >= 0f) ? 1 : -1;
            p.itemAnimation = p.itemAnimationMax = 2;
            p.itemTime = 2;

            if (!fired && t >= windup)
            {
                fired = true;
                fire?.Invoke();
                p.velocity.X += p.direction * 1.2f;
            }

            t++;
            return t < (windup + strike + recover);
        }

        public void ApplyLowerBody(Player p)
        {
            int dir = p.direction;
            if (finalThrow)
            {
                if (t <= windup) p.bodyRotation = dir * 0.03f;
                else if (t <= windup + strike) p.bodyRotation = dir * 0.05f;
                else p.bodyRotation *= 0.9f;
            }
            else
            {
                if (t <= windup) p.bodyRotation = dir * 0.02f;
                else if (t <= windup + strike) p.bodyRotation = dir * 0.03f;
                else p.bodyRotation *= 0.8f;
            }
        }

        public void ApplyUpperBody(Player p)
        {
            int dir = p.direction;
            float neutral = aim.ToRotation() - MathHelper.PiOver2;
            float back = neutral - MathHelper.ToRadians(finalThrow ? 60f : 40f) * dir;
            float forward = neutral + MathHelper.ToRadians(finalThrow ? 30f : 15f) * dir;

            if (t <= windup)
            {
                float u = EaseOutCubic(t / (float)Math.Max(1, windup));
                p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(neutral, back, u));
            }
            else if (t <= windup + strike)
            {
                float u = EaseOutCubic((t - windup) / (float)Math.Max(1, strike));
                p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(back, forward, u));
            }
            else (t <= windup + strike)
            {
                float u = EaseOutCubic((t - (windup + strike)) / (float)Math.Max(1, recover));
                p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(back, forward, u));
            }
        }
        static float EaseOutCubic(float t) => 1f - (float)System.Math.Pow(1f - Clamp01(t), 3);
        static float Clamp01(float v) => v < 0 ? 0 : (v > 1 ? 1 : v);


    }
}
