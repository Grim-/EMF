using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class BasicResourceGeneDef : GeneDef
    {
        public AbilityResourceDef primaryResourceDef;
        public List<PawnExtraResources> additionalResources;
        public BasicResourceGeneDef()
        {
            geneClass = typeof(Gene_BasicResource);
            this.resourceGizmoType = typeof(GeneGizmo_PrimaryResource);
        }

        public bool HasResource(AbilityResourceDef otherResourceDef)
        {
            if (otherResourceDef == primaryResourceDef)
            {
                return true;
            }

            return additionalResources.Any(x => x.resourceDef == otherResourceDef);
        }
    }
}
