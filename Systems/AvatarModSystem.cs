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
using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography.X509Certificates;
using ATLAMod.Systems.Players;
using ATLAMod.UI.BreathMeter;

namespace ATLAMod.Systems
{
    public class AvatarModSystem : ModSystem
    {

        //BENDINGCHOOSEUI+BENDINGMOVESUI INTERFACES
        public BendingChooseUI bendingChooseUI;
        public BendingMovesUI bendingMovesUI;
        private UserInterface bendingChooseInterface;
        private UserInterface bendingMovesInterface;

        //BREATHMETER - FIREBENDER INTERFACES
        public BreathMeter breathMeter;
        private UserInterface breathMeterInterface;


        public static Texture2D WhitePixel;


        public override void Load()
        {
            if (!Main.dedServ)
            {
                //BENDINGCHOOSEUI+BENDINGMOVESUI
                bendingChooseUI = new BendingChooseUI();
                bendingMovesUI = new BendingMovesUI();

                bendingChooseInterface = new UserInterface();
                bendingMovesInterface = new UserInterface();

                bendingChooseUI.Activate();
                bendingMovesUI.Activate();

                //BREATHMETER
                breathMeter = new BreathMeter();
                breathMeterInterface = new UserInterface();
                breathMeter.Activate();
                breathMeterInterface.SetState(breathMeter);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {

            //BENDINGCHOOSEUI
            if (bendingChooseUI.Visible)
            {
                bendingChooseInterface?.Update(gameTime);
            }

            //BENDINGMOVESUI
            if (bendingMovesUI.Visible)
            {
                bendingMovesInterface?.Update(gameTime);
            }

            //BREATHMETER
            if (Main.LocalPlayer.GetModPlayer<BendingPlayer>().hasLearnedFire == true)
            {
                breathMeter.Visible = true;
                breathMeterInterface?.Update(gameTime);
            }
            else
            {
                breathMeter.Visible = false;
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

            if (mouseTextIndex != -1)
            {

                //BENDINGCHOOSEUI
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

                //BENDINGMOVESUI
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

                if (breathMeter.Visible)
                {
                    layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                        "AvatarMod: Breath Meter UI",
                        () =>
                    {
                        breathMeterInterface.Draw(Main.spriteBatch, new GameTime());
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

        public void ShowBendingMovesUI(BendingPlayer.BendingStyle style)
        {
            bendingMovesUI.SetStyle(style);
            bendingMovesUI.Show();
            bendingMovesInterface?.SetState(bendingMovesUI);
        }

        public override void PreUpdatePlayers()
        {
            if (bendingChooseUI != null && bendingChooseUI.Visible)
            {
                Player player = Main.LocalPlayer;
                player.controlUp = false;
                player.controlDown = false;
                player.controlLeft = false;
                player.controlRight = false;
                player.controlJump = false;
                player.controlUseItem = false;
                player.controlUseTile = false;
                player.controlHook = false;
                player.controlMount = false;
                player.controlQuickHeal = false;
                player.controlQuickMana = false;
                player.controlSmart = false;
                player.controlTorch = false;
                player.controlInv = false;
                player.releaseUseItem = false;
                player.releaseUseTile = false;
                player.controlThrow = false;
                player.controlMap = false;

                player.velocity = Vector2.Zero;
            }             
        }

        public override void OnWorldUnload()
        {
            bendingChooseUI?.Hide();
            bendingMovesUI?.Hide();
        }
    }
}
