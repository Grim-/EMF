using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class ResourceAssociatedHediffExtension : DefModExtension
    {
        public AbilityResourceDef associatedResourceDef;
        public bool scaleAllDamage = true;
        public List<DamageDef> damageTypesToScale;
        public List<DamageDef> damageTypesToIgnore;
    }
}
