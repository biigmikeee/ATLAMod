using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using ATLAMod.Systems.Players;

namespace ATLAMod.UI.BendingScroll
{
    public class BendingMovesUI : UIState
    {
        public bool Visible;

        private int inputBlockFrames = 0;
        private UIImage blackOverlay;
        private UIImage scrollPanel;

        private BendingPlayer.BendingStyle currentStyle;
        public override void OnInitialize()
        {
            blackOverlay = new UIImage(ModContent.Request<Texture2D>("ATLAMod/Assets/UITextures/whitePixel"));
            blackOverlay.Width.Set(Main.screenWidth, 0f);
            blackOverlay.Height.Set(Main.screenHeight, 0f);
            blackOverlay.Left.Set(0, 0f);
            blackOverlay.Top.Set(0, 0f);
            blackOverlay.ImageScale = 10000;
            blackOverlay.Color = Color.Black * 0.55f;
            Append(blackOverlay);

            scrollPanel = new UIImage(ModContent.Request<Texture2D>("ATLAMod/Assets/UITextures/bendingChooseUI/bendingChooseUIbackground"));
            scrollPanel.Left.Set((Main.screenWidth - 450) / 2f, 0f);
            scrollPanel.Top.Set((Main.screenHeight - 700) / 2f, 0f);
            scrollPanel.Width.Set(450, 0f);
            scrollPanel.Height.Set(700, 0f);
            Append(scrollPanel);

        }

        public void SetStyle(BendingPlayer.BendingStyle style)
        {
            currentStyle = style;
            scrollPanel.RemoveAllChildren();

            switch (style)
            {
                case BendingPlayer.BendingStyle.Fire:
                    LoadFireMoves();
                    break;
                case BendingPlayer.BendingStyle.Water:
                    LoadWaterMoves();
                    break;
                case BendingPlayer.BendingStyle.Earth:
                    LoadEarthMoves();
                    break;
                case BendingPlayer.BendingStyle.Air:
                    LoadAirMoves();
                    break;
            }
        }

        private void LoadFireMoves()
        {
            Main.NewText("firemoves");
        }

        private void LoadWaterMoves()
        {
            Main.NewText("watermoves");
        }

        private void LoadEarthMoves()
        {
            Main.NewText("earthmoves");
        }

        private void LoadAirMoves()
        {
            Main.NewText("airmoves");
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
                base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible)
            {
                if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    Hide();
                }

                base.Update(gameTime);
            }
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
