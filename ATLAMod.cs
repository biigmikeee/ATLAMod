using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ATLAMod
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class ATLAMod : Mod
	{
		public static ModKeybind UseBreathKeyBind;
		public static ModKeybind BreatheKeybind;

        public override void Load()
        {
            UseBreathKeyBind = KeybindLoader.RegisterKeybind(this, "Use Breath TEST", "Z");
            BreatheKeybind = KeybindLoader.RegisterKeybind(this, "Breathe", "X");
        }

        public override void Unload()
        {
            UseBreathKeyBind = null;
            BreatheKeybind = null;
        }
    }
}
