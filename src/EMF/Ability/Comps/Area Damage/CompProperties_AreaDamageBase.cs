using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public abstract class CompProperties_AreaDamageBase : CompProperties_AbilityEffect
    {
        public EffecterDef cellEffecterDef = null;
        public DamageParameters damageParms = DamageParameters.Default();
        public FriendlyFireSettings friendlyFireParms = FriendlyFireSettings.AllAttack();
        public TargetingParameters targetParms = TargetingParameters.ForAttackAny();
    }

    public abstract class CompAbilityEffect_AreaDamageBase : CompAbilityEffect
    {
        protected CompProperties_AreaDamageBase Props => (CompProperties_AreaDamageBase)props;

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawFieldEdges(GetAffectedCells(parent.pawn.Position, target.Cell, parent.pawn.Map).ToList());
        }

        protected abstract IEnumerable<IntVec3> GetAffectedCells(IntVec3 origin, IntVec3 target, Map map);

        protected virtual IEnumerable<IntVec3> OrderCells(IEnumerable<IntVec3> cells, IntVec3 origin)
        {
            return cells.OrderBy(c => c.DistanceTo(origin));
        }

        protected virtual void SpawnCellImpactEffect(IntVec3 cell, Map map)
        {
            if (Props.cellEffecterDef != null)
            {
                Props.cellEffecterDef.Spawn(cell, map);
            }
            else
            {
                EffecterDefOf.ImpactSmallDustCloud.Spawn(cell, map);
            }
        }


        protected virtual void DealDamageToThingsInCell(IntVec3 cell, Map map)
        {
            foreach (var things in cell.GetThingList(map).ToArray())
            {
                if (this.Props.friendlyFireParms.CanTargetThing(this.parent.pawn, things) && this.Props.targetParms.CanTarget(things))
                {
                    if (Props.damageParms != null)
                    {
                        Props.damageParms.DealDamageTo(this.parent.pawn, things);
                    }
                }
            }
        }
    }
}
