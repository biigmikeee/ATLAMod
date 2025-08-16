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

        private Texture2D breathBorderGlow;
        private float borderGlowFrameTimer = 0f;
        private int borderGlowFrame = 0;
        private int borderGlowFrameCount = 7;
        private float borderGlowFrameSpeed = 0.1f;

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

            breathFillGlow = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathFillGlowTEST"));
            breathFillGlow.Left.Set(0f, 0f); //relative to container
            breathFillGlow.Top.Set(0f, 0f); // relative to container
            breathFillGlow.Width.Set(UI_WIDTH, 0f);
            breathFillGlow.Height.Set(UI_HEIGHT, 0f);
            breathFillGlow.Color = Color.Transparent;
            breathFillContainer.Append(breathFillGlow);      

            breathBorderGlow = ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeterPassiveGlowAnimated").Value;

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
            float newWidth = MathHelper.Lerp(currentWidth, targetWidth, lerpSpeed * (float)Main.frameRate);
            breathFillContainer.Width.Set(newWidth, 0f);

            //handle breathe animation, (make the fill bar glow when breathing, using breath)
            UpdateGlowEffect(player);
            UpdatePassiveRegenGlow(player);
            


            base.Update(gameTime);
        }

        //FILLBAR GLOW EFFECTS
        private void UpdateGlowEffect(BendingPlayer player)
        {
            Color glowColor = Color.White;

            bool shouldGlow = false;
            float pulseSpeed = 4f;

            if (player.isActivelyBreathing)
            {
                shouldGlow = true;
                pulseSpeed = 8f;
            }

            else if(player.breathRegenTimer >= 180 && player.breath < player.maxBreath)
            {
                shouldGlow = true;
                pulseSpeed = 4f;
            }

            if (shouldGlow)
            {
                float pulseIntensity = 0.5f + 0.5f * (float)Math.Sin(animationTimer * pulseSpeed);

                float maxOpacity = 0.45f;
                float glowOpacity = pulseIntensity * maxOpacity;

                breathFillGlow.Color = Color.White * glowOpacity;
            }
            else
            {
                breathFillGlow.Color = Color.Transparent;
            }

            breathFill.Color = glowColor;
        }

        //BORDER EFFECTS - PASSIVE, FULL, ACTIVE BREATHING

        //PASSIVE BORDER GLOW HANDLING

        private bool showPassiveGlow = false;
        private void UpdatePassiveRegenGlow(BendingPlayer player)
        {            
            if (player.breathRegenTimer >= 180 && player.breath < player.maxBreath)
            {
                showPassiveGlow = true;

                borderGlowFrameTimer += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                if (borderGlowFrameTimer >= borderGlowFrameSpeed)
                {
                    borderGlowFrameTimer -= borderGlowFrameSpeed;
                    borderGlowFrame = (borderGlowFrame + 1) % borderGlowFrameCount;
                }
            }
            else
            {
                showPassiveGlow = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
            {
                return;
            }
            base.Draw(spriteBatch);

            if (showPassiveGlow)
            {                
                int frameWidth = breathBorderGlow.Width;
                int frameHeight = breathBorderGlow.Height / borderGlowFrameCount;

                Rectangle sourceRect = new Rectangle(0, borderGlowFrame * frameHeight, frameWidth, frameHeight);

                Vector2 position = new Vector2(UI_LEFT - 5, UI_TOP - 5);
                spriteBatch.Draw(breathBorderGlow, position, sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

        }
    }
}
