using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class TransformationFormTracker : IExposable, IThingHolder
    {
        private ThingOwner<Thing> innerContainer;
        private ThingWithComps storedPreviousForm = null;
        private ThingWithComps storedNextForm = null;
        private IThingHolder parent;

        public bool HasPrevious => storedPreviousForm != null;
        public bool HasNext => storedNextForm != null;
        public ThingWithComps PreviousForm => storedPreviousForm;
        public ThingWithComps NextForm => storedNextForm;

        public IThingHolder ParentHolder => parent;

        public TransformationFormTracker(IThingHolder parent)
        {
            this.parent = parent;
            innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public void StorePreviousForm(ThingWithComps previousForm)
        {
            if (previousForm == null) return;

            if (!innerContainer.Contains(previousForm))
            {
                DespawnOrRemoveForm(previousForm);
                storedPreviousForm = previousForm;
                innerContainer.TryAddOrTransfer(previousForm);
            }
        }

        public void StoreNextForm(ThingWithComps nextForm)
        {
            if (nextForm == null) return;

            if (!innerContainer.Contains(nextForm))
            {
                DespawnOrRemoveForm(nextForm);
                storedNextForm = nextForm;
                innerContainer.TryAddOrTransfer(nextForm);
            }
        }

        public ThingWithComps RetrievePreviousForm()
        {
            if (!HasPrevious) return null;

            ThingWithComps form = storedPreviousForm;
            storedPreviousForm = null;
            innerContainer.Remove(form);
            return form;
        }

        public ThingWithComps RetrieveNextForm()
        {
            if (!HasNext) return null;

            ThingWithComps form = storedNextForm;
            storedNextForm = null;
            innerContainer.Remove(form);
            return form;
        }

        public void RemoveFromContainer(ThingWithComps form)
        {
            if (form != null)
            {
                innerContainer.Remove(form);
            }
        }

        private void DespawnOrRemoveForm(Thing form)
        {
            if (form is Pawn formPawn && formPawn.Dead)
            {
                if (formPawn.Corpse != null && formPawn.Corpse.Spawned)
                {
                    formPawn.Corpse.DeSpawn();
                }
            }

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

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref storedPreviousForm, "storedPreviousForm");
            Scribe_References.Look(ref storedNextForm, "storedNextForm");

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