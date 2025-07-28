using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_AbilityDamageInRadius : CompProperties_AreaDamageBase
    {
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
            Map map = parent.pawn.Map;
            IntVec3 start = parent.pawn.Position;

            StageVisualEffect.CreateRadialStageEffect(start, Props.radius, map, 3, (cell, targetMap, currentSection) =>
            {
                foreach (var things in cell.GetThingList(targetMap))
                {
                    if (this.Props.friendlyFireParms.CanTargetThing(this.parent.pawn, things) && this.Props.targetParms.CanTarget(things))
                    {
                        things.TakeDamage(GetDamage(this.parent.pawn, things));
                    }
                }
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
