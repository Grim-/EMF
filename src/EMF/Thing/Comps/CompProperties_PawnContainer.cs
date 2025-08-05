using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EMF
{


    public class CompProperties_PawnContainer : CompProperties
    {
        public int storageLimit = -1;

        public CompProperties_PawnContainer()
        {
            compClass = typeof(Comp_PawnContainer);
        }
    }
    public class Comp_PawnContainer : ThingComp
    {
        protected ThingOwner<Thing> storage;
        public bool HasStored => storage.InnerListForReading.Count > 0;

        public Comp_PawnContainer()
        {
            storage = new ThingOwner<Thing>(this.ParentHolder, LookMode.Deep);
        }

        public void StorePawn(Pawn pawn)
        {
            if (HasStored)
            {
                Log.Message("cant store pawn, storage is full.");
                return;
            }

            Map lastMap = pawn.Map;
            IntVec3 lastPosition = pawn.Position;

            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }

            if (storage.TryAdd(pawn, true))
            {
                //stored pawn
                Log.Message("stored pawn");
            }
            else
            {
                GenSpawn.Spawn(pawn, lastPosition, lastMap);
                Log.Message("Failed to store pawn, respawning");
            }
        }

        public void ReleasePawn(IntVec3 position, Map map)
        {
            if (storage == null || !HasStored)
            {
                return;
            }

            Pawn pawn = (Pawn)this.storage.InnerListForReading.FirstOrDefault();
            if (pawn == null)
            {
                return;
            }

            if (this.storage.TryDrop(pawn, position, map, ThingPlaceMode.Near, out Thing droppedThing, null))
            {
                Log.Message($"Released {droppedThing.LabelShort}");
            }
            else
            {
                Log.Message("Failed to release pawn");
            }
        }


        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();

            StringBuilder sb = new StringBuilder();


            if (this.storage.InnerListForReading.Count > 0)
            {
                foreach (var item in this.storage.InnerListForReading)
                {
                    sb.Append($"{item.LabelShort}\r\n");
                }
            }
            else
            {
                sb.Append("Nothing stored");
            }


            return baseString + sb.ToString();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Deep.Look(ref storage, "pawnStore");
        }
    }
}
