using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EMF
{
    public class TransformableForm
    {
        public ThingDef thingDef;
        public PawnKindDef pawnKindDef;

        public bool IsPawn => pawnKindDef != null;
        public bool IsThing => thingDef != null && pawnKindDef == null;

        public string Label => IsPawn ? pawnKindDef.LabelCap : thingDef?.LabelCap ?? "Unknown";
        public Texture2D Icon => IsPawn ? pawnKindDef.race.uiIcon : thingDef?.uiIcon;
    }

    public class CompProperties_TransformingThing : CompProperties
    {
        public List<TransformableForm> transformableForms = new List<TransformableForm>();

        public CompProperties_TransformingThing()
        {
            compClass = typeof(CompTransformingThing);
        }
    }

    public class CompTransformingThing : CompTransformingBase, IDrawEquippedGizmos
    {
        public CompProperties_TransformingThing Props => (CompProperties_TransformingThing)props;

        public bool CanRevert => HasPrevious;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            OriginalEquipOwner = null;
        }

        public bool CanTransformTo(TransformableForm form)
        {
            if (form == null)
                return false;

            // Can't transform to current form
            if (parent.def == form.thingDef)
                return false;

            return Props.transformableForms.Contains(form);
        }

        public Thing TransformTo(TransformableForm form)
        {
            if (!CanTransformTo(form))
                return null;

            ThingWithComps targetForm = null;

            // Check if we already have this form stored
            if (HasNext && MatchesForm(storedNextForm, form))
            {
                targetForm = storedNextForm;
            }
            else
            {
                targetForm = CreateForm(form);
                if (targetForm == null)
                    return null;

                TransferBasicProperties(parent, targetForm);

                if (targetForm.TryGetComp(out CompTransformingThing transformingComp))
                {
                    transformingComp.OriginalEquipOwner = this.OriginalEquipOwner ?? GetEquipper();
                }
            }

            return Transform(targetForm, isReverting: false);
        }

        private bool MatchesForm(ThingWithComps thing, TransformableForm form)
        {
            if (form.IsPawn && thing is Pawn pawn)
            {
                return pawn.kindDef == form.pawnKindDef;
            }
            else if (form.IsThing)
            {
                return thing.def == form.thingDef;
            }
            return false;
        }

        private ThingWithComps CreateForm(TransformableForm form)
        {
            if (form.IsPawn)
            {
                var request = new PawnGenerationRequest(
                    form.pawnKindDef,
                    faction: OriginalEquipOwner?.Faction ?? Faction.OfPlayer,
                    context: PawnGenerationContext.NonPlayer,
                    forceGenerateNewPawn: true,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: false,
                    colonistRelationChanceFactor: 0f
                );

                return PawnGenerator.GeneratePawn(request);
            }
            else if (form.IsThing)
            {
                return (ThingWithComps)ThingMaker.MakeThing(form.thingDef, parent.Stuff);
            }

            return null;
        }

        protected override void SpawnForm(ThingWithComps form)
        {
            SpawnFormWithContext(form, GetEquipper());
        }

        private void TransferBasicProperties(Thing sourceThing, Thing targetThing)
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
                defaultDesc = $"Revert to {storedPreviousForm.def.LabelCap}",
                icon = storedPreviousForm.def.uiIcon,
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
            if (CanRevert && storedPreviousForm != null)
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
            if (CanRevert && storedPreviousForm != null)
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
                parts.Add($"Can revert to: {storedPreviousForm.def.LabelCap}");
            }

            if (HasNext)
            {
                parts.Add($"Stored next: {storedNextForm.def.LabelCap}");
            }

            return string.Join("\n", parts);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }
    }
}