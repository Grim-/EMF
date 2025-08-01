using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EMF
{
    // Job for transforming into building
    public class JobDriver_TransformIntoBuilding : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil prepare = new Toil();
            prepare.defaultCompleteMode = ToilCompleteMode.Delay;
            prepare.defaultDuration = 60;
            prepare.WithProgressBar(TargetIndex.A, () => (float)prepare.actor.jobs.curDriver.ticksLeftThisToil / 60f);
            yield return prepare;

            // Transform
            Toil transform = new Toil();
            transform.initAction = delegate
            {
                CompPawnTransformable comp = pawn.TryGetComp<CompPawnTransformable>();
                comp?.Transform();
            };
            yield return transform;
        }
    }
}
