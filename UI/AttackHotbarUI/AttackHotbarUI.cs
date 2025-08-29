using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ATLAMod.Systems.Players;
using ATLAMod.Systems.Attacks;


namespace ATLAMod.UI.AttackHotbar
{
    public class AttackHotbarUI : UIState
    {
        private const int SLOT_SIZE = 52;
        private const int SLOT_SPACING = 56;

        private static Texture2D slotEmpty;
        private static Texture2D slotSelected;
        private static Texture2D slotNormal;
        private static Texture2D slotLocked;
        private static Texture2D connectorLine;
        private static Texture2D expansionHint;

        private static Texture2D fireHotbar;
        //private static Texture2D waterHotbar;
        //work on other elements later

        private float smoothExpansion = 0f;
        private float hintPulse = 0f;
    }
}
