using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public abstract class Comp_Grower : ThingComp
    {
        #region Properties

        protected Building_GrowableStructure ParentBuilding => parent as Building_GrowableStructure;
        protected GrowableStructureDef Def => ParentBuilding.Def;
        protected StructureLayoutDef LayoutDef => ParentBuilding.LayoutDef;

        protected int currentStage = 0;
        public int CurrentStageFriendly => currentStage + 1;
        protected int ticksUntilNextAction = 0;

        protected ThingDef overrideWallStuff;
        protected TerrainDef overrideFloorStuff;
        protected ThingDef overrideDoorStuff;
        protected ThingDef overrideFurnitureStuff;

        protected GrowingSkipFlags skipFlags = GrowingSkipFlags.none;
        protected bool includeNaturalTerrain = false;

        // Settings
        protected int baseTicksBetweenActions;
        protected bool growingEnabled = false;
        public bool IsGrowingEnabled => growingEnabled;
        #endregion

        #region Abstract Methods


        public virtual void DoGrowerTick()
        {
            if (LayoutDef == null)
                return;

            int stageCount = LayoutDef.stages.Count;
            if (stageCount <= 0 || currentStage >= stageCount)
                return;

            if (!growingEnabled)
            {
                return;
            }

            if (currentStage >= this.LayoutDef.stages.Count)
            {
                return;
            }
        }


        public virtual void SetGrowing(bool IsEnabled)
        {
            growingEnabled = IsEnabled;
        }

        protected virtual void BuildTerrain(TerrainPlacement terrain)
        {
            if (terrain.terrain == null)
                return;

            if (!CanBuildTerrain(terrain))
                return;

            IntVec3 pos = CalculateCenteredPosition(terrain.position);
            if (!pos.InBounds(ParentBuilding.Map))
                return;

            if (ParentBuilding.IsCellOccupiedByParentBuilding(pos))
                return;

            TerrainDef terrainToUse = overrideFloorStuff ?? terrain.terrain;
            ParentBuilding.Map.terrainGrid.SetTerrain(pos, terrainToUse);
            FleckMaker.ThrowDustPuff(pos, ParentBuilding.Map, 0.5f);
        }

        protected virtual void BuildThing(ThingPlacement placement, ThingDef stuffOverride = null)
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


        public abstract void Initialize();


        protected virtual void OnStageStarted(int stageIndex)
        {
            //Messages.Message($"{ParentBuilding.Label} has started building stage {stageIndex + 1}", MessageTypeDefOf.PositiveEvent);
        }


        protected virtual void OnStageBuildComplete(int stageIndex)
        {
            //Messages.Message($"{ParentBuilding.Label} has finished building stage {stageIndex + 1}", MessageTypeDefOf.PositiveEvent);
        }
        #endregion

        #region Common Methods
        public void SetStage(int newIndex)
        {
            currentStage = Mathf.Clamp(newIndex, 0, LayoutDef.stages.Count - 1);
            OnStageStarted(currentStage);
        }

        public bool IsLastStage()
        {
            return currentStage >= LayoutDef.stages.Count - 1;
        }

        public abstract bool IsStageFullyBuilt();

        public void SetSkipFlags(GrowingSkipFlags flags)
        {
            skipFlags = flags;
        }

        public void ToggleSkipFlag(GrowingSkipFlags flag)
        {
            skipFlags ^= flag;
        }

        public void SetIncludeNaturalTerrain(bool include)
        {
            includeNaturalTerrain = include;
        }

        protected IntVec3 CalculateCenteredPosition(IntVec2 relativePos)
        {
            return ParentBuilding.CalculateCenteredPosition(relativePos);
        }

        protected bool CanBuildTerrain(TerrainPlacement placement)
        {
            if (placement.terrain == null)
                return false;

            if (!includeNaturalTerrain && placement.terrain.natural)
                return false;

            return true;
        }

        protected bool CanBuildThing(ThingPlacement placement, IntVec3 pos, Map map)
        {
            if (placement.thing == null || !placement.thing.BuildableByPlayer)
                return false;

            Building existingBuilding = pos.GetFirstBuilding(map);
            if (existingBuilding != null)
            {
                if (existingBuilding.Faction == Faction.OfPlayer)
                    return false;
            }

            foreach (Thing thing in map.thingGrid.ThingsListAt(pos))
            {
                if (thing.def.building?.isNaturalRock == true)
                    return false;
            }

            return true;
        }

        protected ThingDef GetMaterialOverride(ThingPlacement placement, BuildingPartType partType)
        {
            if (!placement.thing.MadeFromStuff)
                return null;

            switch (partType)
            {
                case BuildingPartType.Wall:
                    return overrideWallStuff;
                case BuildingPartType.Door:
                    return overrideDoorStuff;
                case BuildingPartType.Furniture:
                    return overrideFurnitureStuff;
                default:
                    return null;
            }
        }

        #endregion

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref currentStage, "currentStage", 0);
            Scribe_Values.Look(ref ticksUntilNextAction, "ticksUntilNextAction", 0);

            Scribe_Values.Look(ref skipFlags, "skipFlags", GrowingSkipFlags.none);
            Scribe_Values.Look(ref includeNaturalTerrain, "includeNaturalTerrain", false);
            Scribe_Values.Look(ref growingEnabled, "growingEnabled", false);

            Scribe_Defs.Look(ref overrideWallStuff, "overrideWallStuff");
            Scribe_Defs.Look(ref overrideFloorStuff, "overrideFloorStuff");
            Scribe_Defs.Look(ref overrideDoorStuff, "overrideDoorStuff");
            Scribe_Defs.Look(ref overrideFurnitureStuff, "overrideFurnitureStuff");
        }
    }
}