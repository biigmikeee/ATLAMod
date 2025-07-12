using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using ATLAMod.Systems.Players;
using ATLAMod.Systems;
using ATLAMod.UI.BendingScroll;
using Terraria.Audio;
using Steamworks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ATLAMod.Items.BendingScroll
{
    public class BendingScroll : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.useTime = 20;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Orange;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(new SoundStyle("ATLAMod/Assets/Sounds/SoundEffects/paperNoise2")
            {
                Volume = 2f,
                Pitch = 0.3f
            });

            BendingPlayer modPlayer = player.GetModPlayer<BendingPlayer>();

            if (modPlayer.chosenStyle == BendingPlayer.BendingStyle.None)
            {
                if (!ModContent.GetInstance<AvatarModSystem>().bendingChooseUI.Visible)
                {
                    ModContent.GetInstance<AvatarModSystem>().ShowChooseUI();
                }
            }
            else
            {
                if(modPlayer.hasLearnedFire == true)
                {
                    if (!ModContent.GetInstance<AvatarModSystem>().bendingMovesUIFire.Visible)
                    {
                        ModContent.GetInstance<AvatarModSystem>().ShowBendingMovesUI();
                    }
                }                
            }

            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 6f);
        }
    }
}
