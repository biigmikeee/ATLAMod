using ATLAMod.Systems.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework;

namespace ATLAMod.Systems.Bending.Moves.Fire
{
    public class FireYoyo : BendingMove
    {
        public FireYoyo() : base(
            id: "fire_yoyo",
            name: "Fire Yoyo",
            style: BendingPlayer.BendingStyle.Fire,
            iconPath: "nothing yet",
            cost: 1,
            cdTicks: 30)
        { Tags = AttackTags.Projectile; }

        public override void OnUse(Player p, BendingPlayer bp)
        {
            base.OnUse(p, bp);
            if (p.whoAmI != Main.myPlayer) return;

            Microsoft.Xna.Framework.Vector2 aim = p.DirectionTo(Main.MouseWorld);

        }
    }
}
