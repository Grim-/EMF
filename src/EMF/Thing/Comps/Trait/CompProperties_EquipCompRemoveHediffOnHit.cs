using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_EquipCompRemoveHediffOnHit : CompProperties
    {
        public bool ApplyToSelf = false;
        public bool ApplyOnTarget = true;
        public float Chance = 0.5f;
        public float Severity = 1f;

        public List<HediffDef> hediffsToRemove;

        public CompProperties_EquipCompRemoveHediffOnHit()
        {
            compClass = typeof(EquipComp_ApplyHediffOnHit);
        }
    }


    public class EquipComp_RemoveHediffOnHit : BaseTraitComp
    {
        public override string TraitName => "Cleanse";

        public override string Description =>
            $"This equipment has a chance ({Mathf.RoundToInt(Props.Chance * 100)}%) to remove a health effect.";

        public new CompProperties_EquipCompRemoveHediffOnHit Props => (CompProperties_EquipCompRemoveHediffOnHit)props;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, ref DamageWorker.DamageResult damageWorkerResult)
        {
            Map map = attacker.Map;

            if (Props.ApplyOnTarget && target.Pawn != null)
            {
                if (Rand.Range(0, 1) <= Props.Chance)
                {
                    foreach (var item in Props.hediffsToRemove)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        Hediff hediff = target.Pawn.health.hediffSet.GetFirstHediffOfDef(item);
                        if (hediff != null)
                        {
                            target.Pawn.health.RemoveHediff(hediff);
                        }
                    }

                    return base.Notify_ApplyMeleeDamageToTarget(target, attacker, ref damageWorkerResult);
                }
            }
            else if (Props.ApplyToSelf && EquippedPawn != null)
            {
                if (Rand.Range(0, 1) <= Props.Chance)
                {
                    foreach (var item in Props.hediffsToRemove)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        Hediff hediff = EquippedPawn.health.hediffSet.GetFirstHediffOfDef(item);
                        if (hediff != null)
                        {
                            EquippedPawn.health.RemoveHediff(hediff);
                        }
                    }

                    return base.Notify_ApplyMeleeDamageToTarget(target, attacker, ref damageWorkerResult);
                }
            }


            return base.Notify_ApplyMeleeDamageToTarget(target, attacker, ref damageWorkerResult);
        }

    }
}
