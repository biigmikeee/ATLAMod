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

namespace ATLAMod.Items
{
    public class BendingScroll : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.useTime = 20;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Orange;            
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(new SoundStyle("ATLAMod/Assets/Sounds/paperNoise1")
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
                if (!ModContent.GetInstance<AvatarModSystem>().bendingMovesUI.Visible)
                {
                    ModContent.GetInstance<AvatarModSystem>().ShowBendingMovesUI();
                }
            }
            
            return true;
        }    
    }
}
