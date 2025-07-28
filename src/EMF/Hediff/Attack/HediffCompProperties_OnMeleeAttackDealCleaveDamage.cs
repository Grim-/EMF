using EMF;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class HediffCompProperties_OnMeleeAttackDealCleaveDamage : HediffCompProperties
    {
        public FloatRange damage;
        public DamageDef damageDef;

        public int length = 6;
        public FloatRange angle = new FloatRange(40, 45);

        public HediffCompProperties_OnMeleeAttackDealCleaveDamage()
        {
            compClass = typeof(HediffComp_OnMeleeAttackDealCleaveDamage);
        }
    }

    public class HediffComp_OnMeleeAttackDealCleaveDamage : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_OnMeleeAttackDealCleaveDamage Props => (HediffCompProperties_OnMeleeAttackDealCleaveDamage)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);

            StageVisualEffect.CreateConalStageEffect(this.parent.pawn.Position, MeleeAttackVerb.CurrentTarget.Cell, Props.length, Props.angle.RandomInRange, this.parent.pawn.Map, 3, (IntVec3 position, Map map, int currentSection) =>
            {
                if (Rand.Value > 0.6f)
                {
                    EffecterDefOf.ImpactSmallDustCloud.Spawn(position, map);
                }

                List<Thing> damageAbleThings = position.GetThingList(map).Where(x => x.def.useHitPoints && x != this.parent.pawn).ToList();
                damageAbleThings.ForEach((x) =>
                {
                    DamageInfo damage = this.parent.pawn.GetAttackDamageForPawn(Props.damageDef, Props.damage.RandomInRange, 0, 1);
                    x.TakeDamage(damage);
                  });
            });
        }
    }
}