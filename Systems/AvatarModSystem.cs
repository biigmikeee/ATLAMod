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

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

            if (mouseTextIndex != -1)
            {
                if (bendingChooseUI.Visible)
                {
                    layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                        "AvatarMod: Bending Choose UI",
                        () =>
                        {
                            bendingChooseInterface?.Draw(Main.spriteBatch, new GameTime());
                            return true;
                        },
                        InterfaceScaleType.UI));
                }

                if (bendingMovesUI.Visible)
                {
                    layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                        "AvatarMod: Bending Moves UI",
                        () =>
                        {
                            bendingMovesInterface?.Draw(Main.spriteBatch, new GameTime());
                            return true;
                        },
                        InterfaceScaleType.UI));
                }
            }
        }

        public void ShowChooseUI()
        {
            bendingChooseUI.Show();
            bendingChooseInterface?.SetState(bendingChooseUI);           
        }

        public void ShowBendingMovesUI()
        {
            bendingMovesUI.Show();
            bendingMovesInterface?.SetState(bendingMovesUI);
        }


    }
}
