using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_ActiveZoneDamage : CompProperties
    {
        public int ticksBetweenDamage = 100;
        public int maxTargets = -1;

        public FloatRange damage = new FloatRange(1, 1);
        public DamageDef damageDef;

        public EffecterDef targetDamageEffecterDef = null;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.AllFriendly();

        public CompProperties_ActiveZoneDamage()
        {
            compClass = typeof(ActiveZoneComp_Damage);
        }
    }

    public class ActiveZoneComp_Damage : ActiveZoneComp
    {
        CompProperties_ActiveZoneDamage Props => (CompProperties_ActiveZoneDamage)props;

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);

            if (ParentZone.IsHashIntervalTick(Props.ticksBetweenDamage))
            {
                int currentTargetCount = 0;

                HashSet<Thing> things = ParentZone.GetCurrentThingsInZone(ref cells);
                foreach (var item in things)
                {
                    if (!item.CanTargetThing(this.parent.Faction, Props.friendlyFireSettings))
                    {
                        continue;
                    }

                    if (Props.maxTargets > 0 && currentTargetCount > Props.maxTargets)
                    {
                        break;
                    }

                    if (item.def.useHitPoints)
                    {
                        if (Props.targetDamageEffecterDef != null)
                        {
                            Props.targetDamageEffecterDef.Spawn(item, item.Map);
                        }

                        item.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange));
                        currentTargetCount++;

                    }
                }
            }

        }
    }
}
