using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using ATLAMod.UI.BendingScroll;
using Terraria.UI;
using Microsoft.Xna.Framework;

namespace ATLAMod.Systems
{
    public class AvatarModSystem : ModSystem
    {
        public BendingChooseUI bendingChooseUI;
        public BendingMovesUI bendingMovesUI;
        private UserInterface bendingChooseInterface;
        private UserInterface bendingMovesInterface;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                bendingChooseUI = new BendingChooseUI();
                bendingMovesUI = new BendingMovesUI();

                bendingChooseInterface = new UserInterface();
                bendingMovesInterface = new UserInterface();

                bendingChooseUI.Activate();
                bendingMovesUI.Activate();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (bendingChooseUI.Visible)
            {
                bendingChooseInterface?.Update(gameTime);
            }

            if (bendingMovesUI.Visible)
            {
                bendingMovesInterface?.Update(gameTime);
            }
        }

        public void ShowChooseUI()
        {
            bendingChooseUI.Show();
        }

        public void ShowBendingMovesUI()
        {
            bendingMovesUI.Show();
        }


    }
}
