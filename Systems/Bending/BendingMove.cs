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
using System.Security.Cryptography.X509Certificates;

namespace ATLAMod.Systems.Bending
{    

    public enum AttackTags
    {
        None = 0,
        Melee = 1 << 0,
        Projectile = 1 << 1,
        Movement = 1 << 2,
        Dash = 1 << 3,        
    }

    public abstract class BendingMove
    {
        public string Id { get; }
        public string Name { get; }
        public BendingPlayer.BendingStyle Style { get; }
        public string IconPath { get; }
        public int Cost;
        public int CooldownTicks;
        public AttackTags Tags { get; protected set; } = AttackTags.None;

        public virtual int BaseDamage => 30;
        protected virtual int EffectiveCooldown(Player p, BendingPlayer bp)
            => (int)System.MathF.Round(CooldownTicks * bp.GetCooldownMult(Tags));
        protected virtual float EffectiveCost(Player p, BendingPlayer bp)
            => Cost * bp.GetCostMult(Tags);
        protected virtual int EffectiveDamage(Player p, BendingPlayer bp)
            => (int)System.MathF.Round(BaseDamage * bp.GetDamageMult(Tags));

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
            if (Style == BendingPlayer.BendingStyle.Fire)
            {
                var cost = EffectiveCost(p, bp);
                if (cost > 0 && !bp.TryConsumeBreath(cost)) return;
            }
            int cd = EffectiveCooldown(p, bp);
            if (cd > 0) bp.SetMoveCooldown(Id, cd);
        }

        public virtual Texture2D GetIcon()
        {
            return Terraria.GameContent.TextureAssets.MagicPixel.Value;
        }
    }
}
