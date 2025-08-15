using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using ATLAMod.Systems.Players;
using Microsoft.Xna.Framework;


namespace ATLAMod.UI.BreathMeter
{
    public class BreathMeter : UIState
    {
        private UIImage breathBorder;
        private UIImage breathFill;
        private UIImage breathFillGlow;
        private UIElement breathFillContainer;
        private UIImage breathBack;

        public bool Visible;
        public float fillPercent = 1f;

        private const float UI_LEFT = 500f;
        private const float UI_TOP = 22f;
        private const float UI_WIDTH = 144f;
        private const float UI_HEIGHT = 46f;

        private float animationTimer = 0f;
        public override void OnInitialize()
        {
            //background layer
            breathBack = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeterBack"));
            breathBack.Left.Set(UI_LEFT, 0f);
            breathBack.Top.Set(UI_TOP + 2, 0f);
            breathBack.Width.Set(UI_WIDTH, 0f);
            breathBack.Height.Set(UI_HEIGHT, 0f);
            Append(breathBack);

            //breathfill container to enable clipping
            breathFillContainer = new UIElement();
            breathFillContainer.Left.Set(UI_LEFT, 0f);
            breathFillContainer.Top.Set(UI_TOP, 0f);
            breathFillContainer.Width.Set(UI_WIDTH, 0f);
            breathFillContainer.Height.Set(UI_HEIGHT, 0f);
            breathFillContainer.OverflowHidden = true;
            Append(breathFillContainer);

            //breathFill bar (clipped by container)
            breathFill = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathFillNew"));
            breathFill.Left.Set(0f, 0f); //relative to container
            breathFill.Top.Set(0f, 0f); // relative to container
            breathFill.Width.Set(UI_WIDTH, 0f);
            breathFill.Height.Set(UI_HEIGHT, 0f);
            breathFillContainer.Append(breathFill);

            breathFillGlow = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathFillNew"));
            breathFillGlow.Left.Set(0f, 0f); //relative to container
            breathFillGlow.Top.Set(0f, 0f); // relative to container
            breathFillGlow.Width.Set(UI_WIDTH, 0f);
            breathFillGlow.Height.Set(UI_HEIGHT, 0f);
            breathFillGlow.Color = Color.Transparent;
            breathFillContainer.Append(breathFillGlow);

            // border
            breathBorder = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeterNew"));
            breathBorder.Left.Set(UI_LEFT, 0f);
            breathBorder.Top.Set(UI_TOP, 0f);
            Append(breathBorder);
        }

        public override void Update(GameTime gameTime)
        {
            var player = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            fillPercent = player.breath;

            animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //updating breathfillcontainer based on percentage
            breathFillContainer.Width.Set(UI_WIDTH * fillPercent, 0f);

            //smoothing animation (dontknowifiwantthisyet)
            float targetWidth = UI_WIDTH * fillPercent;
            float currentWidth = breathFillContainer.Width.Pixels;
            float lerpSpeed = 8;
            float newWidth = MathHelper.Lerp(currentWidth, targetWidth, lerpSpeed);
            breathFillContainer.Width.Set(newWidth, 0f);

            //handle breathe animation, (make the fill bar glow when breathing, using breath)
            UpdateGlowEffect(player);

            //handle notenoughbreath animation, (shake screen?, shake meter?, flash?, darken?)


            base.Update(gameTime);
        }

        private void UpdateGlowEffect(BendingPlayer player)
        {
            Color glowColor = Color.White;

            bool shouldGlow = false;
            float pulseSpeed = 2f;

            if (player.isActivelyBreathing)
            {
                shouldGlow = true;
                pulseSpeed = 6f;
            }

            else if(player.breathRegenTimer >= 180 && player.breath < player.maxBreath)
            {
                shouldGlow = true;
                pulseSpeed = 2f;
            }

            if (shouldGlow)
            {
                float pulseIntensity = 0.5f + 0.5f * (float)Math.Sin(animationTimer * pulseSpeed);

                float maxOpacity = 0.6f;
                float glowOpacity = pulseIntensity * maxOpacity;

                breathFillGlow.Color = Color.White * glowOpacity;
            }
            else
            {
                breathFillGlow.Color = Color.Transparent;
            }

            breathFill.Color = glowColor;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
            {
                return;
            }
            base.Draw(spriteBatch);

        }
    }
}
