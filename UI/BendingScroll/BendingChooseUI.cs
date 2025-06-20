using ATLAMod.Systems.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ATLAMod.UI.BendingScroll
{
    public class BendingChooseUI : UIState
    {
        public bool Visible;

        public override void OnInitialize()
        {
            //buttons            
        }

        private void ChooseFire(UIMouseEvent evt, UIElement listeningElement)
        {
            BendingPlayer modPlayer = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            modPlayer.chosenStyle = BendingPlayer.BendingStyle.Fire;
            modPlayer.hasLearnedFire = true;

            Main.NewText("You can now learn Firebending.");

            Hide();
        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }
    }
}
