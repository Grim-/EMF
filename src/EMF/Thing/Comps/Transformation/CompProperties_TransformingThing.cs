using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace EMF
{

    public class CompProperties_TransformingThing : CompProperties
    {
        public List<TransformableForm> transformableForms = new List<TransformableForm>();

        public CompProperties_TransformingThing()
        {
            compClass = typeof(CompTransformingThing);
        }
    }

    public class CompTransformingThing : ThingComp, IThingHolder, IDrawEquippedGizmos
    {
        private TransformationFormTracker formTracker;

        public TransformationFormTracker FormTracker => formTracker;
        public Pawn OriginalEquipOwner = null;
        protected IntVec3 lastPosition;
        protected Map lastMap;

        public bool HasPrevious => formTracker?.HasPrevious ?? false;
        public bool HasNext => formTracker?.HasNext ?? false;
        public bool CanRevert => HasPrevious;

        public CompProperties_TransformingThing Props => (CompProperties_TransformingThing)props;

        public CompTransformingThing()
        {
            formTracker = new TransformationFormTracker(this);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (formTracker == null)
            {
                formTracker = new TransformationFormTracker(this);
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (OriginalEquipOwner == null || pawn != OriginalEquipOwner)
                OriginalEquipOwner = pawn;
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

        public virtual Thing Transform(ThingWithComps targetForm, bool isReverting = false)
        {
            if (targetForm == null)
                return null;

            CaptureCurrentLocation();
            DespawnOrRemoveForm(parent);

            formTracker.RemoveFromContainer(targetForm);

            if (targetForm is ThingWithComps thing)
            {
                CompTransformingThing comp = thing.GetComp<CompTransformingThing>();
                if (comp != null)
                {
                    if (isReverting)
                    {
                        comp.formTracker.StoreNextForm(parent);
                    }
                    else
                    {
                        comp.formTracker.StorePreviousForm(parent);
                    }
                }
            }

            TryAssignOriginalOwner(targetForm);
            SpawnForm(targetForm);
            PostFormSpawned(targetForm, GetEquipper());
            return targetForm;
        }

        public Thing Revert()
        {
            if (!HasPrevious)
                return null;

            ThingWithComps previousForm = formTracker.RetrievePreviousForm();
            return Transform(previousForm, isReverting: true);
        }

        protected Pawn GetEquipper()
        {
            if(OriginalEquipOwner != null)
                return OriginalEquipOwner;

            if (parent.ParentHolder is Pawn_EquipmentTracker tracker)
                return tracker.pawn;
            if (parent.ParentHolder is Pawn_ApparelTracker apparelTracker)
                return apparelTracker.pawn;

            return null;
        }

        protected virtual void PostFormSpawned(ThingWithComps form, Pawn equipper)
        {
            Log.Message("Post Form Spawned");
        }

        protected virtual void SpawnForm(ThingWithComps form)
        {
            Pawn equipper = GetEquipper();

            if (form is Pawn pawn)
            {
                if (pawn.Dead)
                {
                    ResurrectionUtility.TryResurrect(pawn);
                }

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

        public bool CanTransformTo(TransformableForm form)
        {
            if (form == null)
                return false;

            if (parent.def == form.thingDef)
                return false;

            return true;
        }

        public Thing TransformTo(TransformableForm form)
        {
            if (!CanTransformTo(form))
                return null;

            ThingWithComps targetForm = null;

            if (HasNext)
            {
                targetForm = formTracker.NextForm;
            }
            else
            {
                targetForm = CreateNewForm(form);
                if (targetForm == null)
                    return null;
            }

            TransformUtil.TransferBasicProperties(parent, targetForm);
            TryAssignOriginalOwner(targetForm);

            return Transform(targetForm, isReverting: false);
        }

        private void TryAssignOriginalOwner(ThingWithComps targetForm)
        {
            if (targetForm.TryGetComp(out CompTransformingThing transformingComp))
            {
                transformingComp.OriginalEquipOwner = this.OriginalEquipOwner ?? GetEquipper();
            }
        }

        private ThingWithComps CreateNewForm(TransformableForm form)
        {
            if (form.IsPawn)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(
                    form.pawnKindDef,
                    faction: OriginalEquipOwner?.Faction ?? Faction.OfPlayer,
                    context: PawnGenerationContext.NonPlayer,
                    forceGenerateNewPawn: true,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: false,
                    colonistRelationChanceFactor: 0f
                );

                Pawn newPawn = PawnGenerator.GeneratePawn(request);
                return newPawn;
            }
            else if (form.IsThing)
            {
                return (ThingWithComps)ThingMaker.MakeThing(form.thingDef, parent.Stuff);
            }

            return null;
        }

        public IEnumerable<Gizmo> GetEquippedGizmos()
        {
            if (parent is Pawn)
            {
                foreach (var gizmo in GetPawnGizmos())
                    yield return gizmo;
            }
            else if (GetEquipper() != null)
            {
                foreach (var gizmo in GetEquipmentGizmos())
                    yield return gizmo;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent is Pawn)
            {
                foreach (var gizmo in GetPawnGizmos())
                    yield return gizmo;
            }
        }

        private Command_Action CreateRevertAction()
        {
            return new Command_Action
            {
                defaultLabel = "Revert",
                defaultDesc = $"Revert to {formTracker.PreviousForm.def.LabelCap}",
                icon = formTracker.PreviousForm.def.uiIcon,
                action = () => Revert()
            };
        }

        private Command_Action CreateTransformAction(TransformableForm form)
        {
            return new Command_Action
            {
                defaultLabel = $"Transform: {form.Label}",
                defaultDesc = form.IsPawn ? form.pawnKindDef.race.description : form.thingDef?.description ?? "",
                icon = form.Icon,
                action = () => TransformTo(form)
            };
        }

        private IEnumerable<Gizmo> GetPawnGizmos()
        {
            if (CanRevert && formTracker.PreviousForm != null)
            {
                yield return CreateRevertAction();
            }

            foreach (var form in Props.transformableForms)
            {
                if (parent.def == form.thingDef)
                    continue;

                yield return CreateTransformAction(form);
            }
        }

        private IEnumerable<Gizmo> GetEquipmentGizmos()
        {
            if (CanRevert && formTracker.PreviousForm != null)
            {
                yield return CreateRevertAction();
            }

            foreach (var form in Props.transformableForms)
            {
                if (parent.def == form.thingDef)
                    continue;

                yield return CreateTransformAction(form);
            }
        }

        public override string CompInspectStringExtra()
        {
            var parts = new List<string>();

            parts.Add($"Forms: {Props.transformableForms.Count}");

            if (HasPrevious && CanRevert)
            {
                parts.Add($"Can revert to: {formTracker.PreviousForm.def.LabelCap}");
            }

            if (HasNext)
            {
                parts.Add($"Stored next: {formTracker.NextForm.def.LabelCap}");
            }

            return string.Join("\n", parts);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            formTracker?.GetChildHolders(outChildren);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return formTracker?.GetDirectlyHeldThings();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref formTracker, "formTracker", this);
            Scribe_References.Look(ref OriginalEquipOwner, "originalEquipOwner");
            Scribe_Values.Look(ref lastPosition, "lastPosition");
            Scribe_References.Look(ref lastMap, "lastMap");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (formTracker == null)
                {
                    formTracker = new TransformationFormTracker(this);
                }
            }
        }
    }
}