using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace ATLAMod.UI.BendingScroll
{
    public class BendingMovesUI : UIState
    {
        public bool Visible;

        public override void OnInitialize()
        {
            //create tabs
            //moves panel
            //load buttons
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
                base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible)
                base.Update(gameTime);
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
