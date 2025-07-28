using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public abstract class CompProperties_AreaDamageBase : CompProperties_AbilityEffect
    {
        public FloatRange damage = new FloatRange(1f, 1f);
        public FloatRange armourPen = new FloatRange(1f, 1f);
        public DamageDef damageDef;
        public bool useWeaponDamageIfAvailable = false;
        public EffecterDef cellEffecterDef = null;
        public EffecterDef damageffecterDef = null;

        public FloatRange weaponDamageMult = new FloatRange(1f, 1f);

        public FriendlyFireSettings friendlyFireParms = FriendlyFireSettings.HostileOnly();
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
                    if (this.Props.damageffecterDef != null)
                    {
                        this.Props.damageffecterDef.Spawn(things.Position, map);
                    }
                    else 
                        EffecterDefOf.Deflect_Metal.Spawn(things.Position, map);

                    things.TakeDamage(GetDamage(this.parent.pawn, things));
                }
            }
        }


        protected virtual DamageInfo GetDamage(Pawn attacker, Thing victim)
        {
            if (Props.useWeaponDamageIfAvailable && attacker.HasWeaponEquipped())
                return attacker.equipment.PrimaryEq.GetWeaponDamage(attacker, this.Props.weaponDamageMult.RandomInRange);

            float amount = Props.damage.RandomInRange;
            var def = Props.damageDef ?? DamageDefOf.Blunt;
            return new DamageInfo(def, amount, Props.armourPen.RandomInRange, -1, this.parent.pawn, null, (Props.useWeaponDamageIfAvailable && attacker.HasWeaponEquipped() ? attacker.equipment.PrimaryEq.parent.def : null));
        }
    }
}
