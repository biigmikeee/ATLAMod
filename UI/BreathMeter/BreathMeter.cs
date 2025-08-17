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
        
        //THIS IS FOR PASSIVE REGEN GLOWING - may be overdoing this but idc
        private int borderGlowFrame = 0;
        private int borderGlowTimer = 0;
        private const int borderGlowSpeed = 6; //ADJUST SPEED BASED ON ANIMATION
        private const int borderGlowFrameCount = 23;
        private enum GlowAnimState
        {
            None, Starting, Idle, Ending
        }
        private GlowAnimState glowState = GlowAnimState.None;
        private readonly int startStartFrame = 0;
        private readonly int startEndFrame = 12;
        private readonly int idleStartFrame = 13;
        private readonly int idleEndFrame = 18;
        private readonly int endStartFrame = 19;
        private readonly int endEndFrame = 22;

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

        private float glowTimer = 0f;
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

            glowTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //updating breathfillcontainer based on percentage
            breathFillContainer.Width.Set(UI_WIDTH * fillPercent, 0f);

            //smoothing animation (dontknowifiwantthisyet)
            float targetWidth = UI_WIDTH * fillPercent;
            float currentWidth = breathFillContainer.Width.Pixels;
            float lerpSpeed = 8f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float newWidth = MathHelper.Lerp(currentWidth, targetWidth, lerpSpeed);
            breathFillContainer.Width.Set(newWidth, 0f);

            //handle breathe animation, (make the fill bar glow when breathing, using breath)
            UpdateGlowEffect(player);
            
        //handling passive regen animation vvvvvv -----------------------------------------------------
            if (player.breathRegenTimer >= 180 && player.breath < player.maxBreath)
            {
                if (glowState == GlowAnimState.None)
                {
                    glowState = GlowAnimState.Starting;
                    borderGlowFrame = startStartFrame;
                }
            }
            else
            {
                if (glowState == GlowAnimState.None)
                {
                    glowState = GlowAnimState.Ending;
                    borderGlowFrame = endStartFrame;
                }
            }

            borderGlowTimer++;
            if(borderGlowTimer >= borderGlowSpeed)
            {
                borderGlowTimer = 0;

                switch (glowState)
                {
                    case GlowAnimState.Starting:
                        if (borderGlowFrame < startEndFrame)
                        {
                            borderGlowFrame++;
                        } 
                        else
                        {
                            glowState = GlowAnimState.Idle;
                            borderGlowFrame = idleStartFrame;
                        }
                        break;
                    case GlowAnimState.Idle:
                        borderGlowFrame++;
                        if (borderGlowFrame > idleEndFrame)
                        {
                            borderGlowFrame = idleStartFrame;
                        }
                        break;
                    case GlowAnimState.Ending:
                        if (borderGlowFrame < endEndFrame)
                        {
                            borderGlowFrame++;
                        }
                        else
                        {
                            glowState = GlowAnimState.None;
                        }
                        break;
                }
            }
    //handling passive regen glow ^^^ ----------------------------------------------------------

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
                float pulseIntensity = 0.5f + 0.5f * (float)Math.Sin(glowTimer * pulseSpeed);

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
                
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
            {
                return;
            }
            base.Draw(spriteBatch);


            var player = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            //MORE FRAMES  SMOOTHER ANIMATION
            var breathBorderGlow = ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/PassiveGlowFinal").Value;

            Vector2 position = new Vector2(UI_LEFT - 10, UI_TOP - 10);            

            if (player.breathRegenTimer >= 180 && player.breath < player.maxBreath)
            {                
                int frameHeight = breathBorderGlow.Height / borderGlowFrameCount;                
                Rectangle sourceRect = new Rectangle(0, borderGlowFrame * frameHeight, breathBorderGlow.Width, frameHeight - 2);
                spriteBatch.Draw(breathBorderGlow, position, sourceRect, Color.White * 0.8f);
            }

        }
    }
}
