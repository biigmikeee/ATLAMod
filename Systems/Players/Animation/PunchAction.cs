using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace ATLAMod.Systems.Players.Animation
{
    public sealed class PunchAction : IAnimAction
    {
        private readonly int windup, strike, recover;
        private readonly Vector2 aim;
        private readonly int bodyWalkIndex;
        private readonly int legWalkIndex;
        private int t;
        private bool fired;
        private readonly System.Action fire;

        public PunchAction (Vector2 aimDir, int windupTicks, int strikeTicks, int recoverTicks, int bodyWalkIndex = 4, int legWalkIndex = 5, System.Action onFire = null)
        {
            aim = aimDir.LengthSquared() > 0 ? Vector2.Normalize(aimDir) : new Vector2(1, 0);
            windup = windupTicks; strike = strikeTicks; recover = recoverTicks;
            this.bodyWalkIndex = bodyWalkIndex; this.legWalkIndex = legWalkIndex;
            fire = onFire;
        }

        public bool Update(Player p)
        {
            p.direction = (aim.X >= 0f) ? 1 : -1;            

            p.itemAnimation = p.itemAnimationMax = 2;
            p.itemTime = 2;

            int strikeStart = windup;
            if (!fired && t >= strikeStart)
            {
                fired = true;
                fire?.Invoke();

                p.velocity.X += p.direction * 1.4f;
            }

            t++;
            return t < (windup + strike + recover);
        }

        public void ApplyLowerBody(Player p)
        {
            int total = windup + strike + recover;

            if (t <= windup)
            {               
                p.bodyRotation = MathHelper.Clamp(p.direction * 0.03f, -0.08f, 0.08f);

                p.bodyFrame.Y = bodyWalkIndex * p.bodyFrame.Height;
                p.legFrame.Y = legWalkIndex * p.legFrame.Height;
            }
            else if(t <= windup + strike)
            {
                float u = (t - windup) / (float)System.Math.Max(1, strike);
                p.bodyRotation = MathHelper.Lerp(p.direction * 0.03f, 0f, u);
            }
            else
            {
                float u = (t - (windup + strike)) / (float)System.Math.Max(1, recover);
                p.bodyRotation = MathHelper.Lerp(0.0f, 0.0f, u);
            }
        }

        public void ApplyUpperBody(Player p)
        {
            p.direction = (aim.X >= 0f) ? 1 : -1;
            int dir = p.direction;

            float neutral = aim.ToRotation() - MathHelper.PiOver2;
            float back = neutral - MathHelper.ToRadians(70f) * dir;
            float forward = neutral + MathHelper.ToRadians(18f) * dir;            

            if (t <= windup)
            {
                float u = EaseOutCubic(t / (float)System.Math.Max(1, windup));
                p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(neutral, back, u));
            }
            else if(t <= windup + strike)
            {
                float u = EaseOutCubic((t - windup) / (float)System.Math.Max(1, strike));
                p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(back, forward, u));
            }
            else
            {
                float u = EaseOutCubic((t - (windup + strike)) / (float)System.Math.Max(1, recover));
                p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(forward, neutral, u));
            }
        }

        static float EaseOutCubic(float x) => 1f - (float)System.Math.Pow(1f - Clamp01(x), 3);
        static float Clamp01(float v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    }
}
