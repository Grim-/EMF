using RimWorld;
using Verse;

namespace EMF
{

    public static class TransformUtil
    {
        public static void TransferBasicProperties(Thing sourceThing, Thing targetThing)
        {
            if (targetThing.def.stackLimit > 1 && sourceThing.def.stackLimit > 1 && !(targetThing is Pawn))
            {
                targetThing.stackCount = sourceThing.stackCount;
            }

            if (sourceThing.TryGetQuality(out QualityCategory quality))
            {
                targetThing.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
            }
        }

        public static void TransferForms(ThingWithComps SourceThing, ThingWithComps TargetThing, bool isReverting)
        {
            if (TargetThing is ThingWithComps thing)
            {
                CompTransformingThing comp = thing.GetComp<CompTransformingThing>();
                if (comp != null)
                {
                    if (isReverting)
                    {
                        comp.FormTracker.StoreNextForm(SourceThing);
                    }
                    else
                    {
                        comp.FormTracker.StorePreviousForm(SourceThing);
                    }
                }
            }
        }
    }
}