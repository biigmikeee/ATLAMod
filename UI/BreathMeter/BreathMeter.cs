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
        private UIElement breathFillContainer;
        private UIImage breathBack;

        public bool Visible;
        public float fillPercent = 1f;

        private const float UI_LEFT = 510f;
        private const float UI_TOP = 24f;
        private const float UI_WIDTH = 140f;
        private const float UI_HEIGHT = 40f;


        public override void OnInitialize()
        {
            //background layer
            breathBack = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeterBack"));
            breathBack.Left.Set(UI_LEFT, 0f);
            breathBack.Top.Set(UI_TOP, 0f);
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
            breathFill = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathFill"));
            breathFill.Left.Set(0f, 0f); //relative to container
            breathFill.Top.Set(0f, 0f); // relative to container
            breathFill.Width.Set(UI_WIDTH, 0f);
            breathFill.Height.Set(UI_HEIGHT, 0f);
            breathFillContainer.Append(breathFill);

            // border
            breathBorder = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeter"));
            breathBorder.Left.Set(UI_LEFT, 0f);
            breathBorder.Top.Set(UI_TOP, 0f);
            Append(breathBorder);
        }

        public override void Update(GameTime gameTime)
        {
            var player = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            fillPercent = player.breath;

            //updating breathfillcontainer based on percentage
            breathFillContainer.Width.Set(UI_WIDTH * fillPercent, 0f);

            //smoothing animation (dontknowifiwantthisyet)
            float targetWidth = UI_WIDTH * fillPercent;
            float currentWidth = breathFillContainer.Width.Pixels;
            float lerpSpeed = 8;
            float newWidth = MathHelper.Lerp(currentWidth, targetWidth, lerpSpeed);
            breathFillContainer.Width.Set(newWidth, 0f);

            base.Update(gameTime);
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
