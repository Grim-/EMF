using RimWorld;
using Verse;

namespace EMF
{
    public class CompProperties_EquipCompScaleableDamage : CompProperties
    {
        public float baseIncrease = 0.2f;
        public float highSkillIncrease = 0.35f;
        public int highSkillThreshold = 18;

        public CompProperties_EquipCompScaleableDamage()
        {
            compClass = typeof(EquipComp_ScaleableDamage);
        }
    }

    public class EquipComp_ScaleableDamage : BaseTraitComp
    {
        public override string TraitName => "Scaling:Melee";

        public override string Description => "This equipment's damage will hit harder the better the pawns melee skill.";

        private CompProperties_EquipCompScaleableDamage Props => (CompProperties_EquipCompScaleableDamage)props;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, ref DamageWorker.DamageResult damageWorkerResult)
        {
            if (EquippedPawn != null)
            {
                int skillLevel = EquippedPawn.skills.GetSkill(SkillDefOf.Melee).Level;
                float damageIncrease = CalculateDamageIncrease(skillLevel);

                float scaledDamage = damageWorkerResult.totalDamageDealt * (1f + damageIncrease);
                Log.Message($"Scaling damage {damageWorkerResult.totalDamageDealt} to {scaledDamage}");
                damageWorkerResult.totalDamageDealt = scaledDamage;
            }
            return base.Notify_ApplyMeleeDamageToTarget(target, attacker, ref damageWorkerResult);
        }

        private float CalculateDamageIncrease(int skillLevel)
        {
            if (skillLevel >= Props.highSkillThreshold)
            {
                return Props.highSkillIncrease;
            }
            else
            {
                return Props.baseIncrease;
            }
        }
    }
}
