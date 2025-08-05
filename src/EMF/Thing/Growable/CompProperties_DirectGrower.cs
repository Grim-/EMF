using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace EMF
{


    public class CompProperties_DirectGrower : CompProperties
    {
        public CompProperties_DirectGrower()
        {
            compClass = typeof(Comp_DirectGrower);
        }
    }

    public class Comp_DirectGrower : Comp_Grower
    {
        CompProperties_DirectGrower Props => (CompProperties_DirectGrower)props;
        private int ticksBetweenPlacements => ParentBuilding.TicksBetweenBuilds;

        private Queue<BuildPlacement> TerrainBuildQueue = new Queue<BuildPlacement>();
        private Queue<BuildPlacement> BuildQueue = new Queue<BuildPlacement>();

        public override void Initialize()
        {
            if (ParentBuilding == null)
                return;
            baseTicksBetweenActions = ticksBetweenPlacements;

            overrideWallStuff = Def.defaultWallStuff;
            overrideFloorStuff = Def.defaultFloorStuff;
            overrideDoorStuff = Def.defaultDoorStuff;
            overrideFurnitureStuff = Def.defaultFurnitureStuff;

            // Start at first stage
            currentStage = 0;
            PopulateQueueForStage(Def.structureLayout.GetStage(currentStage));
            SetGrowing(true);
        }
        public override void DoGrowerTick()
        {
            base.DoGrowerTick();

            ticksUntilNextAction++;
            if (ticksUntilNextAction >= ticksBetweenPlacements)
            {
                if (IsStageFullyBuilt())
                {
                    if (!HasNextStage())
                    {
                        ParentBuilding.OnFinishedGrowing();
                        SetGrowing(false);
                        return;
                    }

                    if (ParentBuilding.removeLastStageOnProgress && ParentBuilding.lastStageThings.Count > 0)
                    {
                        ParentBuilding.DestroyLastStagePlacedThings();
                    }
                    else
                    {
                        ParentBuilding.ResetLastStagePlacedThings();
                    }

                    SetStage(currentStage + 1);
                }
                else
                {
                    BuildNext();
                }

                ticksUntilNextAction = 0;
            }
        }

        private void PopulateQueueForStage(BuildingStage stage)
        {
            BuildQueue = new Queue<BuildPlacement>();
            TerrainBuildQueue = new Queue<BuildPlacement>();
            if (LayoutDef == null)
            {
                return;
            }

            // Build in a specific order: terrain -> walls -> doors -> power -> furniture -> other
            foreach (var item in stage.terrain)
            {
                TerrainBuildQueue.Enqueue(new BuildPlacement()
                {
                    partType = BuildingPartType.Floor,
                    placement = item
                });
            }

            foreach (var item in stage.walls)
            {
                BuildQueue.Enqueue(new BuildPlacement()
                {
                    partType = BuildingPartType.Wall,
                    placement = item
                });
            }

            foreach (var item in stage.doors)
            {
                BuildQueue.Enqueue(new BuildPlacement()
                {
                    partType = BuildingPartType.Door,
                    placement = item
                });
            }

            foreach (var item in stage.power)
            {
                BuildQueue.Enqueue(new BuildPlacement()
                {
                    partType = BuildingPartType.Power,
                    placement = item
                });
            }

            foreach (var item in stage.furniture)
            {
                BuildQueue.Enqueue(new BuildPlacement()
                {
                    partType = BuildingPartType.Furniture,
                    placement = item
                });
            }

            foreach (var item in stage.other)
            {
                BuildQueue.Enqueue(new BuildPlacement()
                {
                    partType = BuildingPartType.Other,
                    placement = item
                });
            }
        }
        protected override void BuildThing(ThingPlacement placement, ThingDef stuffOverride = null)
        {
            if (placement.thing == null)
                return;

            IntVec3 pos = CalculateCenteredPosition(placement.position);

            if (!pos.InBounds(ParentBuilding.Map))
                return;

            if (ParentBuilding.IsCellOccupiedByParentBuilding(pos) || !CanBuildThing(placement, pos, ParentBuilding.Map))
                return;

            bool canPlace = true;
            List<Thing> existingThings = pos.GetThingList(ParentBuilding.Map);
            foreach (Thing t in existingThings)
            {
                if (t == ParentBuilding)
                    continue;

                if (t.def == placement.thing || t.def.entityDefToBuild == placement.thing)
                {
                    canPlace = false;
                    break;
                }
            }

            if (!canPlace)
                return;

            ThingDef stuffToUse = placement.thing.MadeFromStuff ?
                                 (stuffOverride ?? placement.stuff) :
                                 placement.stuff;

            Thing thing = ThingMaker.MakeThing(placement.thing, stuffToUse);

            Thing placedThing = GenSpawn.Spawn(thing, pos, ParentBuilding.Map, placement.rotation);

            if (placedThing != null)
            {
                ParentBuilding.AddPlacedThing(placedThing);
                ParentBuilding.AddLastStagePlacedThing(placedThing);
                FleckMaker.ThrowMetaPuffs(new TargetInfo(pos, ParentBuilding.Map));
            }
        }
        private bool BuildNext()
        {
            if (LayoutDef == null || currentStage < 0)
                return false;

            BuildingStage stage = LayoutDef.stages[currentStage];

            if (stage == null)
                return false;


            if (!HasAnythingToBuild())
            {
                return false;
            }


            if (TerrainBuildQueue.Count > 0)
            {
                BuildPlacement buildPlacement = TerrainBuildQueue.Dequeue();
                BuildTerrain((TerrainPlacement)buildPlacement.placement);
            }
            else if (BuildQueue.Count > 0)
            {
                BuildPlacement buildPlacement = BuildQueue.Dequeue();
                BuildThing(buildPlacement.placement, GetMaterialOverride(buildPlacement.placement, buildPlacement.partType));
            }

            return false;
        }
        public bool HasAnythingToBuild()
        {
            if (TerrainBuildQueue == null)
            {
                return false;
            }

            if (BuildQueue == null)
            {
                return false;
            }


            if (TerrainBuildQueue.Count > 0)
            {
                return true;
            }

            if (BuildQueue.Count > 0)
            {
                return true;
            }

            return false;
        }

        public override bool IsStageFullyBuilt()
        {
            if (LayoutDef == null || currentStage < 0)
                return true;

            return !HasAnythingToBuild();
        }
        public bool HasNextStage()
        {
            if (LayoutDef == null || currentStage < 0)
            {
                return false;
            }

            return currentStage < this.LayoutDef.stages.Count - 1;
        }
        protected override void OnStageStarted(int stageIndex)
        {
            base.OnStageStarted(stageIndex);
            PopulateQueueForStage(LayoutDef.GetStage(currentStage));
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();

            int stageCount = LayoutDef.stages.Count;
            if (stageCount > 0 && currentStage >= 0)
            {
                sb.AppendLine("Stage: " + (CurrentStageFriendly) + "/" + stageCount);
            }

            return sb.ToString();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref TerrainBuildQueue, "terrainBuildQueue");
            Scribe_Collections.Look(ref BuildQueue, "buildQueue");
        }
    }
}