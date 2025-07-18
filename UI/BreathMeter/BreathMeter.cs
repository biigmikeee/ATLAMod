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
        private UIImage breathBack;
        public bool Visible;
        public float fillPercent = 1f;

        public override void OnInitialize()
        {
            breathBack = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeterBack"));
            breathBack.Left.Set(510f, 0f);
            breathBack.Top.Set(24f, 0f);
            breathBack.Width.Set(140, 0f);
            breathBack.Height.Set(40, 0f);
            Append(breathBack);

            breathFill = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathFill"));
            breathFill.Left.Set(510f, 0f);
            breathFill.Top.Set(24f, 0f);
            breathFill.Width.Set(140, 0f);
            breathFill.Height.Set(40, 0f);
            Append(breathFill);

            breathBorder = new UIImage(ModContent.Request<Texture2D>("ATLAMod/UI/BreathMeter/BreathMeter"));
            breathBorder.Left.Set(510f, 0f);
            breathBorder.Top.Set(24f, 0f);
            Append(breathBorder);
        }

        public override void Update(GameTime gameTime)
        {
            var player = Main.LocalPlayer.GetModPlayer<BendingPlayer>();

            fillPercent = player.breath;
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
