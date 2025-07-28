using RimWorld;
using Verse;

namespace EMF
{
    public class HediffCompProperties_Petrification : HediffCompProperties
    {
        public HediffCompProperties_Petrification()
        {
            compClass = typeof(HediffComp_Petrification);
        }
    }


    public class HediffComp_Petrification : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.parent.Severity >= 1.0f && this.Pawn.Spawned)
            {
                AttemptPetrification();
            }
        }

        private void AttemptPetrification()
        {
            if (Pawn == null || !Pawn.Spawned)
                return;

            Map map = Pawn.Map;
            IntVec3 position = Pawn.Position;

            PetrifiedStatue statue = PetrifiedStatue.PetrifyPawn(Pawn, position, map);

            if (statue != null)
            {
                Messages.Message($"{Pawn.LabelShort} has been turned to stone.",
                    statue, MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}