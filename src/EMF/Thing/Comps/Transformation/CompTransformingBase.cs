//using RimWorld;
//using System.Collections.Generic;
//using Verse;

//namespace EMF
//{
//    public abstract class CompTransformingBase : ThingComp, IThingHolder
//    {
//        protected ThingOwner<Thing> innerContainer;
//        public Pawn OriginalEquipOwner = null;
//        protected IntVec3 lastPosition;
//        protected Map lastMap;

//        protected ThingWithComps storedPreviousForm = null;
//        protected ThingWithComps storedNextForm = null;

//        public bool HasPrevious => storedPreviousForm != null;
//        public bool HasNext => storedNextForm != null;

//        public CompTransformingBase()
//        {
//            innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
//        }

//        public override void Notify_Equipped(Pawn pawn)
//        {
//            base.Notify_Equipped(pawn);
//            if (OriginalEquipOwner == null)
//                OriginalEquipOwner = pawn;
//        }

//        public void StorePreviousForm(ThingWithComps previousForm)
//        {
//            if (!innerContainer.Contains(previousForm))
//            {
//                DespawnOrRemoveForm(previousForm);
//                storedPreviousForm = previousForm;
//                innerContainer.TryAddOrTransfer(previousForm);
//            }
//        }

//        public void StoreNextForm(ThingWithComps nextForm)
//        {
//            if (!innerContainer.Contains(nextForm))
//            {
//                DespawnOrRemoveForm(nextForm);
//                storedNextForm = nextForm;
//                innerContainer.TryAddOrTransfer(nextForm);
//            }
//        }

//        protected void CaptureCurrentLocation()
//        {
//            if (parent.Spawned)
//            {
//                lastPosition = parent.Position;
//                lastMap = parent.Map;
//            }
//            else if (parent.ParentHolder is Pawn_EquipmentTracker tracker && tracker.pawn != null)
//            {
//                lastPosition = tracker.pawn.Position;
//                lastMap = tracker.pawn.Map;
//            }
//            else if (parent.ParentHolder is Pawn_ApparelTracker apparelTracker && apparelTracker.pawn != null)
//            {
//                lastPosition = apparelTracker.pawn.Position;
//                lastMap = apparelTracker.pawn.Map;
//            }
//            else if (parent is Pawn pawn && pawn.Spawned)
//            {
//                lastPosition = pawn.Position;
//                lastMap = pawn.Map;
//            }
//        }

//        protected void DespawnOrRemoveForm(Thing form)
//        {
//            if (form is Pawn formPawn && formPawn.Dead)
//            {
//                if (formPawn.Corpse != null && formPawn.Corpse.Spawned)
//                {
//                    formPawn.Corpse.DeSpawn();
//                }
//            }

//            if (form.Spawned)
//            {
//                form.DeSpawn();
//            }
//            else if (form.ParentHolder is Pawn_EquipmentTracker tracker)
//            {
//                tracker.Remove((ThingWithComps)form);
//            }
//            else if (form.ParentHolder is Pawn_ApparelTracker apparelTracker)
//            {
//                apparelTracker.Remove((Apparel)form);
//            }
//        }

//        public virtual Thing Transform(ThingWithComps targetForm, bool isReverting = false)
//        {
//            if (targetForm == null)
//                return null;

//            CaptureCurrentLocation();
//            DespawnOrRemoveForm(parent);

//            innerContainer.Remove(targetForm);

//            if (targetForm is ThingWithComps thing)
//            {
//                CompTransformingBase comp = thing.GetComp<CompTransformingBase>();
//                if (comp != null)
//                {
//                    if (isReverting)
//                    {
//                        comp.StoreNextForm(parent);
//                    }
//                    else
//                    {
//                        comp.StorePreviousForm(parent);
//                    }
//                }
//            }

//            SpawnForm(targetForm);
//            PostFormSpawned(targetForm, GetEquipper());
//            return targetForm;
//        }

//        public Thing Revert()
//        {
//            if (!HasPrevious || HasPrevious && storedNextForm is Pawn storedPReviousPAwn && storedPReviousPAwn.Dead)
//                return null;

//            ThingWithComps previousForm = storedPreviousForm;
//            storedPreviousForm = null;

//            return Transform(previousForm, isReverting: true);
//        }

//        protected Pawn GetEquipper()
//        {
//            if (parent.ParentHolder is Pawn_EquipmentTracker tracker)
//                return tracker.pawn;
//            if (parent.ParentHolder is Pawn_ApparelTracker apparelTracker)
//                return apparelTracker.pawn;
//            return OriginalEquipOwner;
//        }


//        protected virtual void PostFormSpawned(ThingWithComps form, Pawn equipper)
//        {
//            Log.Message("Post Form Spawned");
//        }

//        protected virtual void SpawnForm(ThingWithComps form)
//        {

//        }

//        public void GetChildHolders(List<IThingHolder> outChildren)
//        {
//            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
//        }

//        public ThingOwner GetDirectlyHeldThings()
//        {
//            return innerContainer;
//        }

//        public override void PostExposeData()
//        {
//            base.PostExposeData();
//            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
//            Scribe_References.Look(ref OriginalEquipOwner, "originalEquipOwner");
//            Scribe_References.Look(ref storedPreviousForm, "storedPreviousForm");
//            Scribe_References.Look(ref storedNextForm, "storedNextForm");
//            Scribe_Values.Look(ref lastPosition, "lastPosition");
//            Scribe_References.Look(ref lastMap, "lastMap");

//            if (Scribe.mode == LoadSaveMode.PostLoadInit)
//            {
//                if (innerContainer == null)
//                {
//                    innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
//                }
//            }
//        }
//    }
//}