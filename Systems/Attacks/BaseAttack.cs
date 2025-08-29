using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ATLAMod.Systems.Players;

namespace ATLAMod.Systems.Attacks
{
    public abstract class BaseAttack
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public BendingPlayer.BendingStyle BendingType { get; protected set; }
        public int UnlockLevel { get; protected set; }
        public string TexturePath { get; protected set; }

        //Animation Stuff
        public int AnimationDuration { get; protected set; }
        public bool RequiresPlayerAnimation { get; protected set; }

        protected BaseAttack(string name, string description, BendingPlayer.BendingStyle bendingType, int unlockLevel, string texturePath)
        {
            Name = name;
            Description = description;
            BendingType = bendingType;
            UnlockLevel = unlockLevel;
            TexturePath = texturePath;
        }

        public virtual bool CanUse(Player player)
        {
            BendingPlayer modPlayer = player.GetModPlayer<BendingPlayer>();

            if (!IsUnlocked(player))
            {
                return false;
            }

            return CanUseStyle(player, modPlayer);
        }

        protected abstract bool CanUseStyle(Player player, BendingPlayer modPlayer);

        public virtual bool Use(Player player, Vector2 targetPosition)
        {
            if (!CanUse(player))
            {
                return false;
            }

            BendingPlayer modPlayer = player.GetModPlayer<BendingPlayer>();

            ConsumeResources(player, modPlayer);

            ExecuteAttack(player, targetPosition);

            return true;
        }

        protected abstract void ConsumeResources(Player player, BendingPlayer modPlayer);
        protected abstract void ExecuteAttack(Player player, Vector2 targetPosition);

        public virtual bool IsUnlocked(Player player)
        {
            return true;
        }
    }

    // Firebending Base Attack Class
    public abstract class FireAttack : BaseAttack
    {
        public float BreathCost { get; protected set; }
        public int CooldownTicks { get; protected set; }

        protected FireAttack(string name, string description, float breathCost, int cooldownTicks, int unlockLevel, string texturePath) : base(name, description, BendingPlayer.BendingStyle.Fire, unlockLevel, texturePath)
        {
            BreathCost = breathCost;
            CooldownTicks = cooldownTicks;
        }

        protected override bool CanUseStyle(Player player, BendingPlayer modPlayer)
        {
            if (!modPlayer.HasEnoughBreath(BreathCost))
            {
                return false;
            }

            //OTHER LOGIC STUFF - cooldown things

            return true;
        }

        protected override void ConsumeResources(Player player, BendingPlayer modPlayer)
        {
            modPlayer.ConsumeBreath(BreathCost);
        }
    }
}
