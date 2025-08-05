using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public abstract class CompTransformingBase : ThingComp, IThingHolder
    {
        protected ThingOwner<Thing> innerContainer;
        public Pawn OriginalEquipOwner = null;
        protected IntVec3 lastPosition;
        protected Map lastMap;

        protected ThingWithComps storedPreviousForm = null;
        protected ThingWithComps storedNextForm = null;

        public bool HasPrevious => storedPreviousForm != null;
        public bool HasNext => storedNextForm != null;

        public CompTransformingBase()
        {
            innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (OriginalEquipOwner == null)
                OriginalEquipOwner = pawn;
        }

        public void StorePreviousForm(ThingWithComps previousForm)
        {
            if (!innerContainer.Contains(previousForm))
            {
                DespawnOrRemoveForm(previousForm);
                storedPreviousForm = previousForm;
                innerContainer.TryAddOrTransfer(previousForm);
            }
        }

        public void StoreNextForm(ThingWithComps nextForm)
        {
            if (!innerContainer.Contains(nextForm))
            {
                DespawnOrRemoveForm(nextForm);
                storedNextForm = nextForm;
                innerContainer.TryAddOrTransfer(nextForm);
            }
        }

        protected void CaptureCurrentLocation()
        {
            if (parent.Spawned)
            {
                lastPosition = parent.Position;
                lastMap = parent.Map;
            }
            else if (parent.ParentHolder is Pawn_EquipmentTracker tracker && tracker.pawn != null)
            {
                lastPosition = tracker.pawn.Position;
                lastMap = tracker.pawn.Map;
            }
            else if (parent.ParentHolder is Pawn_ApparelTracker apparelTracker && apparelTracker.pawn != null)
            {
                lastPosition = apparelTracker.pawn.Position;
                lastMap = apparelTracker.pawn.Map;
            }
            else if (parent is Pawn pawn && pawn.Spawned)
            {
                lastPosition = pawn.Position;
                lastMap = pawn.Map;
            }
        }

        protected void DespawnOrRemoveForm(Thing form)
        {
            if (form.Spawned)
            {
                form.DeSpawn();
            }
            else if (form.ParentHolder is Pawn_EquipmentTracker tracker)
            {
                tracker.Remove((ThingWithComps)form);
            }
            else if (form.ParentHolder is Pawn_ApparelTracker apparelTracker)
            {
                apparelTracker.Remove((Apparel)form);
            }
        }

        public Thing Transform(ThingWithComps targetForm, bool isReverting = false)
        {
            if (targetForm == null)
                return null;

            CaptureCurrentLocation();

            Pawn equipper = GetEquipper();

            DespawnOrRemoveForm(parent);

            innerContainer.Remove(targetForm);

            if (targetForm is ThingWithComps thing)
            {
                CompTransformingBase comp = thing.GetComp<CompTransformingBase>();
                if (comp != null)
                {
                    if (isReverting)
                    {
                        comp.StoreNextForm(parent);
                    }
                    else
                    {
                        comp.StorePreviousForm(parent);
                    }
                }
            }

            SpawnFormWithContext(targetForm, equipper);

            return targetForm;
        }

        public Thing Revert()
        {
            if (!HasPrevious)
                return null;

            ThingWithComps previousForm = storedPreviousForm;
            storedPreviousForm = null;

            return Transform(previousForm, isReverting: true);
        }

        public Thing Advance()
        {
            if (!HasNext)
                return null;

            ThingWithComps nextForm = storedNextForm;
            storedNextForm = null;

            return Transform(nextForm, isReverting: false);
        }

        protected Pawn GetEquipper()
        {
            if (parent.ParentHolder is Pawn_EquipmentTracker tracker)
                return tracker.pawn;
            if (parent.ParentHolder is Pawn_ApparelTracker apparelTracker)
                return apparelTracker.pawn;
            return OriginalEquipOwner;
        }

        protected void SpawnFormWithContext(ThingWithComps form, Pawn equipper)
        {
            if (form is Pawn pawn)
            {
                GenSpawn.Spawn(pawn, lastPosition, lastMap);
            }
            else if (form is ThingWithComps equipment && equipper != null)
            {
                if (equipment.def.IsWeapon)
                {
                    if (equipper.equipment != null)
                    {
                        equipper.equipment.AddEquipment(equipment);
                    }
                    else
                    {
                        GenSpawn.Spawn(equipment, lastPosition, lastMap);
                    }
                }
                else if (equipment is Apparel apparel)
                {
                    if (equipper.apparel != null && equipper.apparel.CanWearWithoutDroppingAnything(apparel.def))
                    {
                        equipper.apparel.Wear(apparel);
                    }
                    else
                    {
                        GenSpawn.Spawn(apparel, lastPosition, lastMap);
                    }
                }
                else
                {
                    GenSpawn.Spawn(equipment, lastPosition, lastMap);
                }
            }
            else
            {
                GenSpawn.Spawn(form, lastPosition, lastMap);
            }
        }

        protected abstract void SpawnForm(ThingWithComps form);

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref OriginalEquipOwner, "originalEquipOwner");
            Scribe_References.Look(ref storedPreviousForm, "storedPreviousForm");
            Scribe_References.Look(ref storedNextForm, "storedNextForm");
            Scribe_Values.Look(ref lastPosition, "lastPosition");
            Scribe_References.Look(ref lastMap, "lastMap");

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