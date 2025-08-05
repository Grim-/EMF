using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_PawnStore : HediffCompProperties
    {
        public bool oneStackOnly = true;

        public HediffCompProperties_PawnStore()
        {
            compClass = typeof(HediffComp_DisableMagic);
        }
    }

    public class HediffComp_PawnStore : HediffComp, IThingHolder
    {
        protected HediffCompProperties_PawnStore Props => (HediffCompProperties_PawnStore)props;

        protected ThingOwner<Thing> innerContainer;

        public HediffComp_PawnStore()
        {
            innerContainer = new ThingOwner<Thing>(this, Props.oneStackOnly, LookMode.Deep);
        }

        public IThingHolder ParentHolder => this.Pawn.ParentHolder;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }


        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (innerContainer == null)
                {
                    innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
                }
            }
        }
    }
}