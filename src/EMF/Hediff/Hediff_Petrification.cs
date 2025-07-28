using RimWorld;
using Verse;

namespace EMF
{
    public class Hediff_Petrification : HediffWithComps
    {
        public override string LabelInBrackets
        {
            get
            {
                return this.Severity.ToStringPercent();
            }
        }

        public override bool ShouldRemove => false;

        public override void PostTick()
        {
            base.PostTick();

            if (this.Severity >= 1.0f && pawn.Spawned)
            {
                AttemptPetrification();
            }
        }

        private void AttemptPetrification()
        {
            if (pawn == null || !pawn.Spawned)
                return;

            Map map = pawn.Map;
            IntVec3 position = pawn.Position;

            PetrifiedStatue statue = PetrifiedStatue.PetrifyPawn(pawn, position, map);
            if (statue != null)
            {
                Messages.Message($"{pawn.LabelShort} has been turned to stone.",
                    statue, MessageTypeDefOf.NegativeEvent);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }



}