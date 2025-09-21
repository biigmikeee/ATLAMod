using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ATLAMod.Systems.Bending
{
    public abstract class BendingDamageClasses : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)        
            => damageClass == DamageClass.Generic ? StatInheritanceData.Full : StatInheritanceData.None;

        public override bool GetEffectInheritance(DamageClass damageClass)
            => damageClass == DamageClass.Generic;                 
    }

    public sealed class FireDamageClass : BendingDamageClasses { }
    public sealed class WaterDamageClass : BendingDamageClasses { }
    public sealed class EarthDamageClass : BendingDamageClasses { }
    public sealed class AirDamageClass : BendingDamageClasses { }
}
