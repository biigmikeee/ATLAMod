using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace ATLAMod.Systems.Players.Animation
{
    public interface IAnimAction
    {
        bool Update(Player p);
        void ApplyLowerBody(Player p);
        void ApplyUpperBody(Player p);
    }

    public sealed class BendingAnimator
    {
        private IAnimAction lowerBody, upperBody, fx;

        public void Play(IAnimAction action)
        {
            lowerBody = action; upperBody = action; fx = action;
        }

        public void Update(Player p)
        {
            if (fx != null && !fx.Update(p)) fx = null;

            lowerBody?.ApplyLowerBody(p);
            upperBody?.ApplyUpperBody(p);

            if (fx == null) { lowerBody = null; upperBody = null; }
        }

        public bool IsBusy => lowerBody != null || upperBody != null || fx != null;
    }
}
