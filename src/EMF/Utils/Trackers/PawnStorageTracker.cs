using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class PawnStorageTracker : IExposable, IThingHolder
    {
        private ThingOwner<Pawn> innerContainer;
        private IThingHolder parent;


        public bool Any => innerContainer.Any;

        public IThingHolder ParentHolder => parent;


        public Pawn FirstStored => innerContainer.InnerListForReading.FirstOrDefault();

        public PawnStorageTracker(IThingHolder parent)
        {
            this.parent = parent;
            innerContainer = new ThingOwner<Pawn>(this, false, LookMode.Deep);
        }

        public void StorePawn(Pawn previousForm)
        {
            if (!innerContainer.Contains(previousForm))
            {
                DespawnOrRemoveForm(previousForm);
                innerContainer.TryAddOrTransfer(previousForm);
            }
        }

        public Pawn GetFirstStored()
        {
            if (innerContainer != null && innerContainer.Any)
            {
                return innerContainer.InnerListForReading.First();
            }

            return null;
        }

        public Pawn RemoveNextStored()
        {
            if (innerContainer != null && innerContainer.Any)
            {
                return RemoveFromContainer(innerContainer.InnerListForReading.First());
            }

            return null;
        }
        public Pawn RemoveFromContainer(Pawn form)
        {
            if (form != null)
            {
                innerContainer.Remove(form);
            }

            return form;
        }

        private void DespawnOrRemoveForm(Pawn form, bool destroyCorpse = true)
        {
            if (form is Pawn formPawn && formPawn.Dead)
            {
                if (formPawn.Corpse != null && formPawn.Corpse.Spawned && destroyCorpse)
                {
                    formPawn.Corpse.DeSpawn();
                }
            }

            if (form.Spawned)
            {
                form.DeSpawn();
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

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (innerContainer == null)
                {
                    innerContainer = new ThingOwner<Pawn>(this, false, LookMode.Deep);
                }
            }
        }
    }
}