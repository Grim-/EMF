using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_PawnStore : HediffCompProperties
    {
        public bool oneStackOnly = true;

        public HediffCompProperties_PawnStore()
        {
            compClass = typeof(HediffComp_PawnStore);
        }
    }

    public class HediffComp_PawnStore : HediffComp, IThingHolder
    {
        protected HediffCompProperties_PawnStore Props => (HediffCompProperties_PawnStore)props;

        protected PawnStorageTracker StorageTracker = null;
        public IThingHolder ParentHolder => this.Pawn;

        public HediffComp_PawnStore()
        {
            StorageTracker = new PawnStorageTracker(this);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return StorageTracker.GetDirectlyHeldThings();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref StorageTracker, "StorageTracker", this);
        }
    }
}