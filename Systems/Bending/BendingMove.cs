using ATLAMod.Systems.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ATLAMod.Systems.Bending
{    

    public abstract class BendingMove
    {
        public string Id { get; }
        public string Name { get; }
        public BendingPlayer.BendingStyle Style { get; }
        public string IconPath { get; }
        public int Cost;
        public int CooldownTicks;

        protected BendingMove(string id, string name, BendingPlayer.BendingStyle style, string iconPath, int cost, int cdTicks)
        {
            Id = id;
            Name = name;
            Style = style;
            IconPath = iconPath;
            Cost = cost;
            CooldownTicks = cdTicks;
        }

        public virtual bool CanUse(Player p, BendingPlayer bp)
        {
            if (!bp.chosenStyle.Equals(Style)) { Main.NewText("WRONG STYLE"); return false; }
            if (bp.IsMoveOnCooldown(Id)) { Main.NewText("still on cooldown"); return false; }

            if (Style == BendingPlayer.BendingStyle.Fire) return bp.breath >= Cost;
            return true;
        }

        public virtual void OnUse(Player p, BendingPlayer bp)
        {
            if (Style == BendingPlayer.BendingStyle.Fire && Cost > 0)
            {
                bp.TryConsumeBreath(Cost);
            }
            if (CooldownTicks > 0) bp.SetMoveCooldown(Id, CooldownTicks);
        }

        public virtual Texture2D GetIcon()
        {
            return Terraria.GameContent.TextureAssets.MagicPixel.Value;
        }
    }
}
