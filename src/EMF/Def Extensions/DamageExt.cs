using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class DamageExt : DefModExtension
    {
        public bool IsPhysical = true;

        public List<AbilityResourceDef> scalingResourceTypes;

        public DamageInfo ScaleDamageForPawn(Pawn pawn, DamageInfo damageToScale, AbilityResourceDef resourceDef)
        {
            return DamageScalingUtility.ScaleDamageInfoWithResource(damageToScale, pawn, resourceDef);
        }
    }
}
