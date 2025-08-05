using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EMF
{
    public class CompProperties_PawnTransformable : CompProperties
    {
        public ThingDef buildingDef;
        public int cooldownTicks = 0;
        public EffecterDef transformEffect;

        public CompProperties_PawnTransformable()
        {
            compClass = typeof(CompPawnTransformable);
        }
    }

    public class CompPawnTransformable : ThingComp
    {
        public CompProperties_PawnTransformable Props => (CompProperties_PawnTransformable)props;
        private int lastTransformTick = -999999;
        private bool transformationReady = true;

        public bool CanTransformNow
        {
            get
            {
                if (!transformationReady)
                    return false;
                if (Props.cooldownTicks > 0 && Find.TickManager.TicksGame - lastTransformTick < Props.cooldownTicks)
                    return false;

                Pawn pawn = parent as Pawn;
                if (pawn == null) 
                    return false;

                if (pawn.Downed || pawn.Dead)
                    return false;
                if (pawn.InMentalState) 
                    return false;

                return true;
            }
        }

        public void Transform()
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null || !CanTransformNow) 
                return;

            IntVec3 position = pawn.Position;
            Rot4 rotation = pawn.Rotation;
            Map map = pawn.Map;

            Thing building = ThingMaker.MakeThing(Props.buildingDef);

            CompBuildingFromPawn buildingComp = building.TryGetComp<CompBuildingFromPawn>();

            GenSpawn.Spawn(building, position, map, rotation);

            if (buildingComp != null)
            {
                buildingComp.StorePawn(pawn);
            }
            else
            {
                pawn.Destroy();
            }

            // Effects
            if (Props.transformEffect != null)
            {
                Props.transformEffect.Spawn(position, map);
            }

            lastTransformTick = Find.TickManager.TicksGame;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Transform",
                    defaultDesc = $"Transform into {Props.buildingDef.label}",
                    action = () =>
                    {
                        if (CanTransformNow)
                        {
                            Job job = JobMaker.MakeJob(EMFDefOf.EMF_JobTransformIntoBuilding);
                            (parent as Pawn)?.jobs?.TryTakeOrderedJob(job);
                        }
                        else
                        {
                            Messages.Message("Cannot transform now", MessageTypeDefOf.RejectInput);
                        }
                    },
                    Disabled = !CanTransformNow,
                    disabledReason = GetDisabledReason()
                };
            }
        }

        private string GetDisabledReason()
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null) 
                return "Invalid pawn";
            if (pawn.Downed) 
                return "Pawn is downed";
            if (pawn.InMentalState)
                return "Pawn is in mental state";
            if (!transformationReady)
                return "Transformation not ready";
            if (Props.cooldownTicks > 0 && Find.TickManager.TicksGame - lastTransformTick < Props.cooldownTicks)
                return "On cooldown";
            return "";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastTransformTick, "lastTransformTick", -999999);
            Scribe_Values.Look(ref transformationReady, "transformationReady", true);
        }
    }

}
