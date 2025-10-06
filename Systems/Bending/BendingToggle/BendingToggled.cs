using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ATLAMod.Systems.Bending.BendingToggle
{
    public class BendingToggled : ModItem
    {
        public override void SetDefaults() 
        {
            Item.width = 38;
            Item.height = 38;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ATLAMod", "Bending Toggled"));
        }
    }
}
