using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;



namespace ATLAMod.UI.BendingScroll
{
    public class glowButton : UIElement
    {
        private Texture2D texture;
        private float targetAlpha = 0f;
        private float currAlpha = 0f;
        private const float fadeSpeed = 0.08f;

        public glowButton(Texture2D texture)
        {
            this.texture = texture;
            Width.Set(texture.Width, 0f);
            Height.Set(texture.Height, 0f);
        }

        public void FadeIn() => targetAlpha = 1f;
        public void FadeOut() => targetAlpha = 0f;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);            
            currAlpha = MathHelper.Lerp(currAlpha, targetAlpha, fadeSpeed);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {            
            if (currAlpha > 0.01f)
            {
                CalculatedStyle dimensions = GetDimensions();
                spriteBatch.Draw(texture, dimensions.Position(), Color.White * currAlpha);
            }
        }
    }
}
