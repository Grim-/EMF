using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class HediffCompProperties_OnAbilityUseIncreaseSeverity : HediffCompProperties_AbilityEffect
    {
        public FloatRange severityIncreaseOnCast = new FloatRange(0.01f, 0.05f);

        public HediffCompProperties_OnAbilityUseIncreaseSeverity()
        {
            compClass = typeof(HediffComp_OnAbilityUseIncreaseSeverity);
        }
    }

    public class HediffComp_OnAbilityUseIncreaseSeverity : HediffComp_AbilityEffect
    {
        public HediffCompProperties_OnAbilityUseIncreaseSeverity Props => (HediffCompProperties_OnAbilityUseIncreaseSeverity)props;

        protected override void OnAbilityUsed(Pawn pawn, RimWorld.Ability ability)
        {
            float chosen = Props.severityIncreaseOnCast.RandomInRange;
            this.parent.Severity += chosen;
            if (pawn != null && pawn.Map != null && pawn.Spawned)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{this.parent.Label} increasing severity by {chosen * 100}%");
            }   
        }

        public override string CompDescriptionExtra => base.CompDescriptionExtra + $"\r\nSeverity increases by {Props.severityIncreaseOnCast.min} - {Props.severityIncreaseOnCast.max} whenever an ability is used.";
    }


}