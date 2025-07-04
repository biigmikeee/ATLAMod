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
using ATLAMod.Systems;

namespace ATLAMod.UI.BendingScroll
{
    public class BendingChooseUI : UIState
    {
        private int buttonHoverCooldownFrames = 0;
        private int inputBlockFrames = 0;
        private UIImage blackOverlay;

        public bool Visible;
        private UIImage scrollPanel;

        private UIElement fireButton;
        private UIElement waterButton;
        private UIElement earthButton;
        private UIElement airButton;
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

            fireButton = CreateBendingButton("Fire", 20f, ChooseFire);
            scrollPanel.Append(fireButton);

            waterButton = CreateBendingButton("Water", 190f, ChooseWater);
            scrollPanel.Append(waterButton);

            earthButton = CreateBendingButton("Earth", 360f, ChooseEarth);
            scrollPanel.Append(earthButton);

            airButton = CreateBendingButton("Air", 530f, ChooseAir);
            scrollPanel.Append(airButton);
        }

        private UIElement CreateBendingButton(string styleName, float top, UIElement.MouseEvent clickAction)
        {
            var container = new UIElement();
            container.Width.Set(230, 0f);
            container.Height.Set(150, 0f);
            container.Left.Set((380 - 140) / 2f, 0f);
            container.Top.Set(top, 0f);

            var buttonTexture = ModContent.Request<Texture2D>($"ATLAMod/Assets/UITextures/bendingChooseUI/bendingChooseUIbuttonFire");
            var button = new UIImageButton(buttonTexture);
            button.Width.Set(230, 0f);
            button.Height.Set(150, 0f);
            button.SetVisibility(1f, 1f);            

            var glowTexture = ModContent.Request<Texture2D>($"ATLAMod/Assets/UITextures/bendingChooseUI/bendingChooseUIbuttonGlowFire", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var glowImage = new glowButton(glowTexture);
            glowImage.Width.Set(glowTexture.Width, 0f);
            glowImage.Height.Set(glowTexture.Height, 0f);
            glowImage.Left.Set(0f, 0f);
            glowImage.Top.Set(0f, 0f);
            glowImage.IgnoresMouseInteraction = true;
            
            button.OnMouseOver += (_, _) =>
            {
                if (buttonHoverCooldownFrames == 0)
                {
                    buttonHoverCooldownFrames = 80;
                    glowImage.FadeIn();
                    SoundEngine.PlaySound(new SoundStyle("ATLAMod/Assets/Sounds/SoundEffects/smallFireWoosh1"));
                }
            };

            button.OnMouseOut += (_, _) =>
            {
                glowImage.FadeOut();
            };

            button.OnLeftMouseDown += (_, _) =>
            {
                if(buttonHoverCooldownFrames == 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("ATLAMod/Assets/Sounds/SoundEffects/bigWoosh1"));
                    button.OnLeftClick += clickAction;
                }                
            };            

            container.Append(button);
            container.Append(glowImage);

            return container;
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

            if (inputBlockFrames > 0)
            {
                return;
            }

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
            inputBlockFrames = 15;
        }

        public void Hide()
        {
            Visible = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)            
                return;
            
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible)
            {
                base.Update(gameTime);

                if (inputBlockFrames > 0)
                {
                    inputBlockFrames--;
                }

                if (buttonHoverCooldownFrames > 0)
                {
                    buttonHoverCooldownFrames--;
                }
            }
        }
    }
}
