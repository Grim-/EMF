using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_ActiveZoneDamage : CompProperties
    {
        public int ticksBetweenDamage = 100;
        public int maxTargets = -1;
        public DamageParameters damageParms;
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
                    if (Props.maxTargets > 0 && currentTargetCount > Props.maxTargets)
                    {
                        break;
                    }

                    if (Props.friendlyFireSettings.CanTargetThing(item, this.ParentZone.Owner))
                    {
                        if (Props.targetDamageEffecterDef != null)
                        {
                            Props.targetDamageEffecterDef.Spawn(item, item.Map);
                        }
                        Props.damageParms.DealDamageTo(this.ParentZone, item);
                        currentTargetCount++;

                    }
                }
            }

        }
    }
}
