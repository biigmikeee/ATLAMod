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
using Terraria.Audio;
using System.Security.Cryptography.X509Certificates;


namespace ATLAMod.UI.BreathMeter
{
    public class BreathMeter : UIState
    {
        private UIImage breathBorder;
        
        //THIS IS FOR PASSIVE REGEN GLOWING - may be overdoing this but idc
        private int borderGlowFrame = 0;
        private int borderGlowTimer = 0;
        private const int borderGlowSpeed = 2; //ADJUST SPEED BASED ON ANIMATION
        private const int borderGlowFrameCount = 34;
        private enum GlowAnimState {None, Starting, Idle, Ending}
        private GlowAnimState glowState = GlowAnimState.None;
        private readonly int startStartFrame = 0;
        private readonly int startEndFrame = 10;
        private readonly int idleStartFrame = 11;
        private readonly int idleEndFrame = 23;
        private readonly int endStartFrame = 24;
        private readonly int endEndFrame = 33;        

        //FOR ACTIVEBREATHING GLOW ANIMATION
        private int abGlowFrame = 0;
        private int abGlowTimer = 0;
        private const int abGlowSpeed = 2;
        private const int abGlowFrameCount = 24;
        private GlowAnimState abGlowState = GlowAnimState.None;
        private readonly int abStartStart = 0;
        private readonly int abStartEnd = 10;
        private readonly int abIdleStart = 11;
        private readonly int abIdleEnd = 18;
        private readonly int abEndStart = 19;
        private readonly int abEndEnd = 23;

        //Other breathmeter UI initializing
        private UIImage breathFill;
        private UIImage breathFillGlow;
        private UIElement breathFillContainer;
        private UIImage breathBack;
        
        public bool Visible;
        public float fillPercent = 100;

        //positioning and size for breathmeter
        private const float UI_LEFT = 500f;
        private const float UI_TOP = 22f;
        private const float UI_WIDTH = 144f;
        private const float UI_HEIGHT = 46f;

        //not sure what this does i dont remember
        private float glowTimer = 0f;

        //hover stuff or something
        private int _prevShakeTicks = 0;
        private UIElement hoverElement;

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

            //breathing glow for BreathFill
            breathFillGlow = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathFillGlowTEST"));
            breathFillGlow.Left.Set(0f, 0f); //relative to container
            breathFillGlow.Top.Set(0f, 0f); // relative to container
            breathFillGlow.Width.Set(UI_WIDTH, 0f);
            breathFillGlow.Height.Set(UI_HEIGHT, 0f);
            breathFillGlow.Color = Color.Transparent;
            breathFillContainer.Append(breathFillGlow);

            //hover tooltip
            hoverElement = new UIElement();
            hoverElement.Left.Set(UI_LEFT, 0f);
            hoverElement.Top.Set(UI_TOP, 0f);
            hoverElement.Width.Set(UI_WIDTH, 0f);
            hoverElement.Height.Set(UI_HEIGHT, 0f);
            Append(hoverElement);

            // border
            breathBorder = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeterNew"));
            breathBorder.Left.Set(UI_LEFT, 0f);
            breathBorder.Top.Set(UI_TOP, 0f);
            Append(breathBorder);            
        }

        public override void Update(GameTime gameTime)
        {
            var player = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            fillPercent = player.breath / player.maxBreath;

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
            UpdatePassiveRegenGlow(player);
            UpdateActiveBreathingGlow(player);

            //handling failbreathanimation
            FailShake(player);            
            

            base.Update(gameTime);
        }

        //FILLBAR GLOW EFFECT
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

        //PASSIVEREGENGLOW EFFECT - still need to smooth animation**
        private void UpdatePassiveRegenGlow(BendingPlayer player)
        {
            if (player.breathRegenTimer >= 180 && player.breath < player.maxBreath)
            {
                if (glowState == GlowAnimState.None)
                {
                    glowState = GlowAnimState.Starting;
                    borderGlowFrame = startStartFrame;

                    //playing initial whoosh sound
                    SoundEngine.PlaySound(new SoundStyle("ATLAMod/Assets/Sounds/SoundEffects/shoo-wung")
                    {
                        Volume = 0.1f,
                        Pitch = 0.1f
                    });
                }
            }
            else
            {
                if (glowState != GlowAnimState.None && glowState != GlowAnimState.Ending)
                {
                    glowState = GlowAnimState.Ending;
                    borderGlowFrame = endStartFrame;
                }
            }

            borderGlowTimer++;
            if (borderGlowTimer >= borderGlowSpeed)
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
        }

        private void UpdateActiveBreathingGlow(BendingPlayer player)
        {
            if (player.isActivelyBreathing)
            {
                if (abGlowState == GlowAnimState.None)
                {
                    abGlowState = GlowAnimState.Starting;
                    abGlowFrame = abStartStart;

                    //playing initial whoosh sound
                    SoundEngine.PlaySound(new SoundStyle("ATLAMod/Assets/Sounds/SoundEffects/igniteSmall1")
                    {
                        Volume = 0.1f,
                        Pitch = 0.1f
                    });
                }
            }
            else
            {
                if (abGlowState != GlowAnimState.None && abGlowState != GlowAnimState.Ending)
                {
                    abGlowState = GlowAnimState.Ending;
                    abGlowFrame = abEndStart;
                }
            }

            abGlowTimer++;
            if (abGlowTimer >= abGlowSpeed)
            {
                abGlowTimer = 0;

                switch (abGlowState)
                {
                    case GlowAnimState.Starting:
                        if (abGlowFrame < abStartEnd)
                        {
                            abGlowFrame++;
                        }
                        else
                        {
                            abGlowState = GlowAnimState.Idle;
                            abGlowFrame = abIdleStart;
                        }
                        break;
                    case GlowAnimState.Idle:
                        abGlowFrame++;
                        if (abGlowFrame > abIdleEnd)
                        {
                            abGlowFrame = abIdleStart;
                        }
                        break;
                    case GlowAnimState.Ending:
                        if (abGlowFrame < abEndEnd)
                        {
                            abGlowFrame++;
                        }
                        else
                        {
                            abGlowState = GlowAnimState.None;
                        }
                        break;
                }
            }
        }

        public void FailShake(BendingPlayer player)
        {
            int shake = player.breathShakeTicks;
            Vector2 shakeOffset = Vector2.Zero;

            if (shake > 0)
            {
                float amp = 2.5f * (shake / 12f);
                shakeOffset = new Vector2(Main.rand.NextFloat(-amp, amp), 0f);
            }
            _prevShakeTicks = shake;

            breathBack.Left.Set(UI_LEFT + shakeOffset.X, 0f);
            breathBack.Top.Set(UI_TOP + 2, 0f);
            breathBack.Recalculate();

            breathFillContainer.Left.Set(UI_LEFT + shakeOffset.X, 0f);            
            breathFillContainer.Top.Set(UI_TOP, 0f);
            breathFillContainer.Recalculate();

            breathBorder.Left.Set(UI_LEFT + shakeOffset.X, 0f);
            breathBorder.Top.Set(UI_TOP, 0f);
            breathBorder.Recalculate();

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
            {
                return;
            }
            base.Draw(spriteBatch);


            var player = Main.LocalPlayer.GetModPlayer<BendingPlayer>();            
            var breathBorderGlow = ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/PassiveGlowNEW").Value;
            var abBorderGlow = ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/activeBreathing").Value;

            Vector2 position = new Vector2(UI_LEFT - 4, UI_TOP - 6);
            Vector2 abPosition = new Vector2(UI_LEFT - 10, UI_TOP - 10);

            if (glowState != GlowAnimState.None)
            {                
                int frameHeight = breathBorderGlow.Height / borderGlowFrameCount;                
                Rectangle sourceRect = new Rectangle(0, borderGlowFrame * frameHeight, breathBorderGlow.Width, frameHeight);
                spriteBatch.Draw(breathBorderGlow, position, sourceRect, Color.White * 0.8f);
            }

            if (abGlowState != GlowAnimState.None)
            {
                int abFrameHeight = abBorderGlow.Height / abGlowFrameCount;
                Rectangle ABsourceRect = new Rectangle(0, abGlowFrame * abFrameHeight, abBorderGlow.Width, abFrameHeight);
                spriteBatch.Draw(abBorderGlow, abPosition, ABsourceRect, Color.White * 0.8f);
            }

            var lp = Main.LocalPlayer;
            if (lp == null || !lp.active) return;

            
            if (hoverElement.IsMouseHovering)
            {
                var bp = lp.GetModPlayer<BendingPlayer>();
                int currentBreath = (int)System.MathF.Round(bp.breath);
                Main.instance.MouseText($"Breath: {currentBreath}/{bp.maxBreath}");                
            }
        }
    }
}
