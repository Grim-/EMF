using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EMF
{
    public class JobDriver_DoWeaponTransformation : JobDriver
    {
        public TransformableForm TargetDef => job.count >= 0 && job.count < Props.transformableForms.Count
            ? Props.transformableForms[job.count]
            : null;

        private CompProperties_TransformingThing Props => TargetA.Thing.TryGetComp<CompTransformingThing>()?.Props;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Wait(30);

            Toil transform = new Toil();
            transform.defaultCompleteMode = ToilCompleteMode.Instant;
            transform.initAction = delegate
            {
                CompTransformingThing comp = TargetA.Thing.TryGetComp<CompTransformingThing>();
                comp?.TransformTo(TargetDef);
            };
            yield return transform;
        }
    }
}
