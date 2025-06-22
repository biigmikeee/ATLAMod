using ATLAMod.Systems.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Audio;
using Terraria.ID;
using Steamworks;

namespace ATLAMod.UI.BendingScroll
{
    public class BendingChooseUI : UIState
    {
        public bool Visible;
        private UIImage scrollPanel;

        private UIImageButton fireButton;
        private UIImageButton waterButton;
        private UIImageButton earthButton;
        private UIImageButton airButton;

        public override void OnInitialize()
        {
            scrollPanel = new UIImage(ModContent.Request<Texture2D>("ATLAMod/Assets/UITextures/choosebackgroundTEST"));
            scrollPanel.Left.Set((Main.screenWidth - 450) / 2f, 0f);
            scrollPanel.Top.Set((Main.screenHeight - 700) / 2f, 0f);
            scrollPanel.Width.Set(450, 0f);
            scrollPanel.Height.Set(700, 0f);
            Append(scrollPanel);

            fireButton = CreateBendingButton("Fire", 100f, ChooseFire);
            scrollPanel.Append(fireButton);

            waterButton = CreateBendingButton("Water", 240f, ChooseWater);
            scrollPanel.Append(waterButton);

            earthButton = CreateBendingButton("Earth", 380f, ChooseEarth);
            scrollPanel.Append(earthButton);

            airButton = CreateBendingButton("Air", 520f, ChooseAir);
            scrollPanel.Append(airButton);
        }

        private UIImageButton CreateBendingButton(string styleName, float top, UIElement.MouseEvent clickAction)
        {
            UIImageButton button = new UIImageButton(ModContent.Request<Texture2D>("ATLAMod/Assets/UITextures/choosebuttoniconTEST"));
            button.Left.Set((450 - 180) / 2f, 0f);
            button.Top.Set(top, 0f);
            button.Width.Set(180, 0f);
            button.Height.Set(120, 0f);

            button.OnLeftClick += clickAction;

            //hover effect
            button.OnMouseOver += (evt, element) =>
            {
                SoundEngine.PlaySound(SoundID.Item4);
                //tooltip or glow effect
            };

            return button;
        }

        private void ChooseFire(UIMouseEvent evt, UIElement listeningElement)
        {
            SetChosenStyle(BendingPlayer.BendingStyle.Fire);
        }

        private void ChooseWater(UIMouseEvent evt, UIElement listeningElement)
        {
            SetChosenStyle(BendingPlayer.BendingStyle.Water);
        }

        private void ChooseEarth(UIMouseEvent evt, UIElement listeningElement)
        {
            SetChosenStyle(BendingPlayer.BendingStyle.Earth);
        }

        private void ChooseAir(UIMouseEvent evt, UIElement listeningElement)
        {
            SetChosenStyle(BendingPlayer.BendingStyle.Air);
        }

        private void SetChosenStyle(BendingPlayer.BendingStyle style)
        {
            BendingPlayer modPlayer = Main.LocalPlayer.GetModPlayer<BendingPlayer>();

            if (modPlayer.chosenStyle == BendingPlayer.BendingStyle.None)
            {
                modPlayer.chosenStyle = style;

                switch (style)
                {
                    case BendingPlayer.BendingStyle.Fire:
                        modPlayer.hasLearnedFire = true;
                        break;
                    case BendingPlayer.BendingStyle.Water:
                        modPlayer.hasLearnedWater = true;
                        break;
                    case BendingPlayer.BendingStyle.Earth:
                        modPlayer.hasLearnedEarth = true;
                        break;
                    case BendingPlayer.BendingStyle.Air:
                        modPlayer.hasLearnedAir = true;
                        break;
                }

                Main.NewText($"You have chosen {style}bending!", Color.Orange);
                modPlayer.hasChosenBending = true;

                Hide();
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                base.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible)
            {
                base.Update(gameTime);
            }
        }
    }
}
