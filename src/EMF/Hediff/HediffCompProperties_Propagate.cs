using RimWorld;
using System.Linq;
using Verse;

namespace EMF
{
    public class HediffCompProperties_Propagate : HediffCompProperties
    {
        public int initialStack = 3;
        public bool removeOnStackEmpty = true;

        public int ticksBetweenPropogation = 300;

        public float radius = 5f;
        public bool canMerge = false;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();

        public HediffCompProperties_Propagate()
        {
            compClass = typeof(HediffComp_Propagate);
        }
    }

    public class HediffComp_Propagate : HediffComp
    {
        private int remainingStack;
        private int propogationTick = 0;

        new public HediffCompProperties_Propagate Props => (HediffCompProperties_Propagate)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            remainingStack = Props.initialStack;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (remainingStack <= 0)
                return;

            if (Pawn?.Map == null)
                return;


            propogationTick++;


            if (propogationTick >= Props.ticksBetweenPropogation)
            {
                if (TryPropagate())
                {
                    Log.Message($"{this.parent.Label} propogated itself to a new host!");
                }
                propogationTick = 0;
            }
        }

        private bool TryPropagate()
        {
            var potentialTargets = GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, Props.radius, true)
                .OfType<Pawn>()
                .Where(p => p.CanTargetThing(Pawn.Faction, Props.friendlyFireSettings))
                .Where(p => Props.canMerge || !p.health.hediffSet.HasHediff(parent.def))
                .ToList();

            if (potentialTargets.Count == 0)
                return false;

            var target = potentialTargets.RandomElement();

            if (target.health.hediffSet.HasHediff(parent.def))
            {
                if (Props.canMerge)
                {
                    var existingHediff = target.health.hediffSet.GetFirstHediffOfDef(parent.def);
                    var existingComp = existingHediff.TryGetComp<HediffComp_Propagate>();
                    if (existingComp != null)
                    {
                        existingComp.remainingStack++;
                        remainingStack--;

                        return true;
                    }
                }
            }
            else
            {
                Hediff newHediff = HediffMaker.MakeHediff(parent.def, target);

                var newComp = newHediff.TryGetComp<HediffComp_Propagate>();
                if (newComp != null)
                {
                    newComp.remainingStack = remainingStack - 1;
                }

                target.health.AddHediff(newHediff);
                remainingStack--;

                return true;
            }


            return false;
        }

        private static bool ShouldTarget(Faction targetFaction, Faction sourceFaction, bool canTargetHostile, bool canTargetFriendly, bool canTargetNeutral)
        {
            if (targetFaction == null)
                return canTargetNeutral;

            if (targetFaction == sourceFaction && canTargetFriendly)
                return true;

            if (canTargetHostile && targetFaction.HostileTo(sourceFaction))
                return true;

            return targetFaction != sourceFaction && !targetFaction.HostileTo(sourceFaction) && canTargetNeutral;
        }

        public override string CompDescriptionExtra => base.CompDescriptionExtra + $"Stacks remaining : {remainingStack}";

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref remainingStack, "remainingStack");
        }
    }
}