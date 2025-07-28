using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_ActiveZoneApplyHediff : CompProperties
    {
        public HediffDef hediff;
        public FloatRange severity = new FloatRange(1f, 1f);
        public bool removeOnLeaveZone = true;
        public EffecterDef onApplyEffect = null;
        public EffecterDef onRemoveEffect = null;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.AllFriendly();

        public CompProperties_ActiveZoneApplyHediff()
        {
            compClass = typeof(Comp_ActiveZoneApplyHediff);
        }
    }

    public class Comp_ActiveZoneApplyHediff : ActiveZoneComp
    {
        protected CompProperties_ActiveZoneApplyHediff Props => (CompProperties_ActiveZoneApplyHediff)props;

        public override void OnThingEnteredZone(ActiveZone zone, Thing thing)
        {
            if (!(thing is Pawn pawn))
                return;

            if (!pawn.health.hediffSet.HasHediff(Props.hediff) && pawn.CanTargetThing(this.parent.Faction, Props.friendlyFireSettings))
            {
                Hediff hediff = pawn.health.GetOrAddHediff(Props.hediff);
                if (hediff != null)
                {
                    hediff.Severity = Props.severity.RandomInRange;
                    if (Props.onApplyEffect != null)
                    {
                        Props.onApplyEffect.Spawn(pawn.Position, pawn.Map, 1f);
                    }
                }
            }
        }

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);


            foreach (var item in ParentZone.PawnsInZoneRead)
            {
                if (item is Pawn pawn)
                {
                    if (!pawn.health.hediffSet.HasHediff(Props.hediff) && pawn.CanTargetThing(this.parent.Faction, Props.friendlyFireSettings))
                    {
                        Hediff hediff = pawn.health.GetOrAddHediff(Props.hediff);
                        if (hediff != null)
                        {
                            hediff.Severity = Props.severity.RandomInRange;
                            if (Props.onApplyEffect != null)
                            {
                                Props.onApplyEffect.Spawn(pawn.Position, pawn.Map, 1f);
                            }
                        }
                    }
                }
            }
        }

        public override void OnThingLeftZone(ActiveZone zone, Thing thing)
        {
            if (!Props.removeOnLeaveZone || !(thing is Pawn pawn))
                return;

            if (thing.CanTargetThing(this.parent.Faction, Props.friendlyFireSettings))
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);

                    if (Props.onRemoveEffect != null)
                    {
                        Props.onRemoveEffect.Spawn(pawn.Position, pawn.Map, 1f);
                    }
                }
            }
        }

        public override void OnZoneDespawned(ActiveZone parentZone, ref List<IntVec3> cells)
        {
            if (!Props.removeOnLeaveZone)
                return;

            var thingsInZone = parentZone.PawnsInZoneRead;
            foreach (var thing in thingsInZone)
            {
                if (thing.CanTargetThing(this.parent.Faction, Props.friendlyFireSettings))
                {
                    Hediff hediff = thing.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                    if (hediff != null)
                    {
                        thing.health.RemoveHediff(hediff);

                        if (Props.onRemoveEffect != null)
                        {
                            Props.onRemoveEffect.Spawn(thing.Position, thing.Map, 1f);
                        }
                    }
                }
            }
        }
    }
}
