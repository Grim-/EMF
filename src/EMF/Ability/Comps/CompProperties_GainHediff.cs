using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_GainHediff : CompProperties_AbilityEffect
    {
        public List<HediffApplicationData> hediffsToApply;
        public CompProperties_GainHediff()
        {
            compClass = typeof(CompAbilityEffect_GainHediff);
        }
    }

    public class HediffApplicationData
    {
        public HediffDef hediffDef;
        public FloatRange initialSeverity = new FloatRange(1, 1);
        public FloatRange severityGain = new FloatRange(0.1f, 0.1f);
    }

    public class CompAbilityEffect_GainHediff : CompAbilityEffect
    {
        public new CompProperties_GainHediff Props => (CompProperties_GainHediff)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            foreach (var item in Props.hediffsToApply)
            {
                if (target.Pawn != null)
                {
                    if (!target.Pawn.health.hediffSet.HasHediff(item.hediffDef))
                    {
                        //initial application
                        Hediff hediff = target.Pawn.health.AddHediff(item.hediffDef);
                        if (hediff != null)
                        {
                            hediff.Severity = item.initialSeverity.RandomInRange;
                        }
                    }
                    else
                    {
                        Hediff hediff = target.Pawn.health.GetOrAddHediff(item.hediffDef);

                        if (hediff != null)
                        {
                            hediff.Severity += item.severityGain.RandomInRange;
                        }
                    }
                }

            }
        }
    }
}
