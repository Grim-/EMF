using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{

    public class CompProperties_BuildingFromPawn : CompProperties
    {
        public bool canReverseTransform = true;
        public EffecterDef reverseEffect;

        public CompProperties_BuildingFromPawn()
        {
            compClass = typeof(CompBuildingFromPawn);
        }
    }

    public class CompBuildingFromPawn : ThingComp, IThingHolder, IThingHolderTickable
    {
        public CompProperties_BuildingFromPawn Props => (CompProperties_BuildingFromPawn)props;

        private Pawn sourcePawn;

        protected ThingOwner<Thing> innerContainer;
        public bool CanTransformBack => innerContainer.Count > 0 && sourcePawn != null && !sourcePawn.Destroyed;

        public bool ShouldTickContents => true;

        public CompBuildingFromPawn()
        {
            innerContainer = new ThingOwner<Thing>(this, true, LookMode.Deep, true);
        }

        public void StorePawn(Pawn pawn)
        {
            sourcePawn = pawn;
            pawn.DeSpawn();
            innerContainer.TryAddOrTransfer(pawn);
        }

        public void TransformBack()
        {
            if (!CanTransformBack)
                return;

         
            IntVec3 position = this.parent.Position;
            Map map = this.parent.Map;
            innerContainer.Remove(sourcePawn);
            GenSpawn.Spawn(sourcePawn, position, map);
            this.parent.Destroy();

            if (Props.reverseEffect != null)
            {
                Props.reverseEffect.Spawn(position, map);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer && Props.canReverseTransform)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Transform Back",
                    defaultDesc = "Transform back into pawn",
                    action = delegate
                    {
                        if (CanTransformBack)
                        {
                            TransformBack();
                        }
                    },
                    Disabled = !CanTransformBack,
                    disabledReason = !CanTransformBack ? "Cannot transform back" : ""
                };
            }
        }


        public override string CompInspectStringExtra()
        {
            if (sourcePawn != null)
            {
                return $"Transformed from: {sourcePawn.Name}";
            }
            return null;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref sourcePawn, "sourcePawn");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }

    }
}
