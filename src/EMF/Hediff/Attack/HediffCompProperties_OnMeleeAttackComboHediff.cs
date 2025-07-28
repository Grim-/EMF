using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_OnMeleeAttackComboHediff : HediffCompProperties
    {
        public List<HediffComboData> combos = new List<HediffComboData>();

        public HediffCompProperties_OnMeleeAttackComboHediff()
        {
            compClass = typeof(HediffComp_OnMeleeAttackComboHediff);
        }
    }

    public class HediffComboData
    {
        public Hediff targetHediff;
        public bool consumesTargetHediff = true;
        public int priority = 100;

        //hediff to apply
        public Hediff newHediff;
        public FloatRange chanceToTrigger = new FloatRange(1, 1);
        public FloatRange intialSeverity = new FloatRange(0.1f, 0.1f);

        //damage to deal
        public FloatRange damage;
        public DamageDef damageDef;

        //chance to stun
    }

    public class HediffComp_OnMeleeAttackComboHediff : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_OnMeleeAttackComboHediff Props => (HediffCompProperties_OnMeleeAttackComboHediff)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);
            if (Target.Pawn != null)
            {
                //if target has a hediff that is a combo
                //if (Target.Pawn.health.hediffSet.hediffs.Any(x => Props.combos.Contains()))
                //{
                //    //find combo with lowest priority and do that one.


                //}
            }
        }
    }
}