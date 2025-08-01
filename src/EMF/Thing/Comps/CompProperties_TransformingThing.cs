using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EMF
{
    public class CompProperties_TransformingThing : CompProperties
    {
        public List<ThingDef> transformableForms = new List<ThingDef>();
        public bool allowReversion = true;
        public bool stackTransformations = true;

        public CompProperties_TransformingThing()
        {
            compClass = typeof(CompTransformingThing);
        }
    }

    public class CompTransformingThing : ThingComp, IThingHolder, IDrawEquippedGizmos
    {
        protected ThingOwner<Thing> innerContainer;

        protected Pawn EquipOwner = null;
        public CompProperties_TransformingThing Props => (CompProperties_TransformingThing)props;
        public bool HasPrevious => !innerContainer.NullOrEmpty();
        public bool CanRevert => HasPrevious && Props.allowReversion;

        public CompTransformingThing()
        {
            innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            EquipOwner = pawn;
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            EquipOwner = null;
        }

        public bool CanTransformTo(ThingDef targetDef)
        {
            if (targetDef == null) 
                return false;

            if (targetDef == parent.def)
                return false;

            return Props.transformableForms.Contains(targetDef);
        }

        public Thing TransformTo(ThingDef targetDef)
        {
            if (!CanTransformTo(targetDef))
                return null;

            Thing newThing = ThingMaker.MakeThing(targetDef, parent.Stuff);
            if (newThing == null)
                return null;

            // Transfer basic properties
            newThing.stackCount = parent.stackCount;
            newThing.HitPoints = Mathf.RoundToInt(parent.HitPoints * ((float)targetDef.BaseMaxHitPoints / parent.def.BaseMaxHitPoints));

            if (this.parent.TryGetQuality(out QualityCategory quality))
            {
                newThing.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
            }

            // Handle transformation stacking
            if (Props.stackTransformations && newThing.TryGetComp(out CompTransformingThing transformingThing))
            {
                Thing snapshot = CreateSnapshot();
                transformingThing.innerContainer.TryAddOrTransfer(snapshot, false);
            }

            // Check if equipped before transforming
            Pawn equipper = null;
            if (parent is ThingWithComps equipment && equipment.ParentHolder is Pawn_EquipmentTracker tracker)
            {
                equipper = tracker.pawn;
            }

            // Replace in world
            if (parent.Spawned)
            {
                IntVec3 position = parent.Position;
                Map map = parent.Map;
                Rot4 rotation = parent.Rotation;

                parent.DeSpawn();
                GenSpawn.Spawn(newThing, position, map, rotation);
            }
            else if (equipper != null && newThing is ThingWithComps newEquipment)
            {
                // Was equipped, remove old and equip new
                equipper.equipment.Remove((ThingWithComps)parent);
                equipper.equipment.AddEquipment(newEquipment);
            }

            return newThing;
        }

        private Thing CreateSnapshot()
        {
            Thing snapshot = ThingMaker.MakeThing(parent.def, parent.Stuff);
            snapshot.stackCount = parent.stackCount;
            snapshot.HitPoints = parent.HitPoints;

            if (parent.TryGetQuality(out QualityCategory quality))
            {
                snapshot.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
            }

            return snapshot;
        }

        public Thing Revert()
        {
            if (!CanRevert)
                return null;

            Thing previousThing = innerContainer.First();
            innerContainer.Remove(previousThing);

            // Check if equipped before reverting
            Pawn equipper = null;
            if (parent is ThingWithComps equipment && equipment.ParentHolder is Pawn_EquipmentTracker tracker)
            {
                equipper = tracker.pawn;
            }

            if (parent.Spawned)
            {
                IntVec3 position = parent.Position;
                Map map = parent.Map;
                Rot4 rotation = parent.Rotation;

                parent.DeSpawn();
                GenSpawn.Spawn(previousThing, position, map, rotation);
            }
            else if (equipper != null && previousThing is ThingWithComps previousEquipment)
            {
                // Was equipped, remove current and equip previous
                equipper.equipment.Remove((ThingWithComps)parent);
                equipper.equipment.AddEquipment(previousEquipment);
            }

            return previousThing;
        }

        public override string CompInspectStringExtra()
        {
            var baseString = base.CompInspectStringExtra();

            baseString += $"forms {Props.transformableForms.Count}";
            if (HasPrevious && CanRevert)
            {
                baseString += $"\nCan revert to: {innerContainer.First().def.LabelCap}";
            }

            return baseString;
        }



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

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (innerContainer == null)
                {
                    innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
                }
            }
        }

        public IEnumerable<Gizmo> GetEquippedGizmos()
        {
            if (CanRevert)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Revert",
                    defaultDesc = $"Revert to {innerContainer.First().def.LabelCap}",
                    icon = innerContainer.First().def.uiIcon,
                    action = () => Revert()
                };
            }

            foreach (var formDef in Props.transformableForms)
            {
                yield return new Command_Action
                {
                    defaultLabel = $"Transform: {formDef.LabelCap}",
                    defaultDesc = formDef.description,
                    icon = formDef.uiIcon,
                    action = () =>
                    {
                        int formIndex = Props.transformableForms.IndexOf(formDef);
                        Job job = JobMaker.MakeJob(EMFDefOf.EMF_JobDoWeaponTransformation, this.parent);
                        job.count = formIndex;
                        this.EquipOwner.jobs.StartJob(job);
                    }
                };
            }
        }
    }
}