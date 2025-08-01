using System.Collections.Generic;
using Verse;

namespace EMF
{    
    
    // Health conditions
    public class HealthEvolutionProperties : EvolutionConditionProperties
    {
        public List<HediffDef> requiredHediffs = new List<HediffDef>();
        public List<HediffDef> forbiddenHediffs = new List<HediffDef>();
    }
    public class EvolutionConditionWorker_Health : BaseEvolutionConditionWorker
    {
        public override bool MeetsCriteria(Pawn pawn)
        {
            var healthProps = props as HealthEvolutionProperties;
            if (healthProps == null) return true;

            if (healthProps.requiredHediffs != null)
            {
                foreach (var hediff in healthProps.requiredHediffs)
                {
                    if (!pawn.health.hediffSet.HasHediff(hediff))
                        return false;
                }
            }

            if (healthProps.forbiddenHediffs != null)
            {
                foreach (var hediff in healthProps.forbiddenHediffs)
                {
                    if (pawn.health.hediffSet.HasHediff(hediff))
                        return false;
                }
            }

            return true;
        }

        public override string GetFailureReason(Pawn pawn)
        {
            var healthProps = props as HealthEvolutionProperties;
            if (healthProps == null) return "Invalid health properties";

            if (healthProps.requiredHediffs != null)
            {
                foreach (var hediff in healthProps.requiredHediffs)
                {
                    if (!pawn.health.hediffSet.HasHediff(hediff))
                        return $"Missing required condition: {hediff.label}";
                }
            }

            if (healthProps.forbiddenHediffs != null)
            {
                foreach (var hediff in healthProps.forbiddenHediffs)
                {
                    if (pawn.health.hediffSet.HasHediff(hediff))
                        return $"Cannot evolve with: {hediff.label}";
                }
            }

            return null;
        }
    }

 
}