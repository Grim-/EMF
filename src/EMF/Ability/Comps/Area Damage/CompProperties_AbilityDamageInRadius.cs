using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_AbilityDamageInRadius : CompProperties_AreaDamageBase
    {
        public EffecterDef explosionEffecterDef = null;
        public float radius = 2f;

        public CompProperties_AbilityDamageInRadius()
        {
            compClass = typeof(CompAbilityEffect_AbilityDamageInRadius);
        }
    }

    public class CompAbilityEffect_AbilityDamageInRadius : CompAbilityEffect_AreaDamageBase
    {
        private CompProperties_AbilityDamageInRadius Props => (CompProperties_AbilityDamageInRadius)props;

        protected override IEnumerable<IntVec3> GetAffectedCells(IntVec3 origin, IntVec3 target, Map map)
        {
            return GenRadial.RadialCellsAround(origin, Props.radius, true);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Map map = parent.pawn.Map;
            IntVec3 start = target.Cell;


            if (Props.explosionEffecterDef != null)
            {
                Props.explosionEffecterDef.Spawn(start, map);
            }

            StageVisualEffect.CreateRadialStageEffect(start, Props.radius, map, 3, (cell, targetMap, currentSection) =>
            {
                if (Props.cellEffecterDef != null)
                {
                    Props.cellEffecterDef.Spawn(cell, targetMap);
                }


                DealDamageToThingsInCell(cell, targetMap);
            });
        }

        protected override DamageInfo GetDamage(Pawn attacker, Thing victim)
        {
            return attacker.GetAttackDamageForPawn(
                Props.damageDef,
                Props.damage.RandomInRange,
                0,
                1,
                Props.useWeaponDamageIfAvailable
            );
        }
    }
}
