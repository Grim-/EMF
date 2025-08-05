using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace EMF
{
    public class Building_GrowableStructure : Plant
    {
        #region Properties and Fields
        private int currentStage = -1;

        private int ticksUntilNextPlacement = 0;

        private bool includeNaturalTerrain = false;
        private ThingDef overrideWallStuff;
        private TerrainDef overrideFloorStuff;
        private ThingDef overrideDoorStuff;
        private ThingDef overrideFurnitureStuff;

        public List<Thing> placedThings = new List<Thing>();
        public List<Thing> lastStageThings = new List<Thing>();

        private bool showPreview = true;
        public bool removeLastStageOnProgress = false;

        private Color previewColor = Color.green * 0.5f;

        private GrowingSkipFlags skipFlags = GrowingSkipFlags.Floors;

        private List<Color> previewColors = new List<Color>()
        {
            Color.cyan,
            Color.yellow,
            Color.magenta,
            Color.green,
            Color.red,
            Color.blue,
            new Color(1.0f, 0.5f, 0.0f),
            new Color(0.5f, 0.0f, 0.5f),
            new Color(0.0f, 0.5f, 0.5f),
            new Color(1.0f, 0.65f, 0.8f)
        };

        private int currentStagePreviewIndex = 0;
        private Comp_RootGrower RootComp => GetComp<Comp_RootGrower>();
        private Comp_Grower Grower => GetComp<Comp_Grower>();

        public IntVec3 LayoutCenter =>
            new IntVec3(
                Position.x + def.size.x / 2,
                Position.y,
                Position.z + def.size.z / 2
            );

        public int TicksBetweenBuilds => Mathf.FloorToInt(Def.ticksBetweenPlacements * GrowthSpeedMultipler);

        public float GrowthSpeedMultipler = 1f;
        public float LightGrowthSpeedFactor = 1f;
        public float GrowthEnergy = 100f;
        public float MaxGrowthEnergy = 100f;

        public GrowableStructureDef Def => (GrowableStructureDef)def;
        public StructureLayoutDef LayoutDef => this.Def.structureLayout;


        public bool DestroyPlacedThingsOnDespawnOrDestroy = true;
        public bool FinishedGrowing => Grower.IsStageFullyBuilt() && Grower.IsLastStage();
        #endregion

        #region Lifecycle Methods
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                Init(Def);
            }
        }


        private void Init(GrowableStructureDef growableDef)
        {
            currentStage = 0;
            overrideWallStuff = growableDef.defaultWallStuff;
            overrideFloorStuff = growableDef.defaultFloorStuff;
            overrideDoorStuff = growableDef.defaultDoorStuff;
            overrideFurnitureStuff = growableDef.defaultFurnitureStuff;

            if (Grower != null)
            {
                Grower.Initialize();
            }
        }


        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (DestroyPlacedThingsOnDespawnOrDestroy) DestroyAllPlacedThings();
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (DestroyPlacedThingsOnDespawnOrDestroy) DestroyAllPlacedThings();
            base.Destroy(mode);
        }

        protected override void Tick()
        {
            base.Tick();

            if (Grower != null && Grower.IsGrowingEnabled)
            {
                Grower.DoGrowerTick();
            }
        }

        #endregion

        public virtual void OnStartedGrowing()
        {

        }

        public virtual void OnFinishedGrowing()
        {
            FleckMaker.ThrowLightningGlow(Position.ToVector3Shifted(), Map, 2f);
            MoteMaker.ThrowText(Position.ToVector3Shifted(), Map, "Growth complete", 3.65f);
        }

        public bool IsCellOccupiedByParentBuilding(IntVec3 worldPos)
        {
            List<IntVec3> occupiedCells = new List<IntVec3>();
            this.OccupiedRect().Cells.ToList().ForEach(c => occupiedCells.Add(c));
            return occupiedCells.Contains(worldPos);
        }
        public void DoRegenTick()
        {
            List<Thing> damagedBuiltThings = placedThings.Where(x => x.def.useHitPoints && x.HitPoints < x.MaxHitPoints).ToList();
            Thing nextThingToRepair = damagedBuiltThings.Last();
            if (nextThingToRepair != null)
            {
                nextThingToRepair.HitPoints += 1;
            }
        }

        public void DoDegenTick()
        {
            if (placedThings != null && placedThings.Count > 0)
            {
                Thing nextThingToDamage = placedThings.Last();
                if (nextThingToDamage != null)
                {
                    nextThingToDamage.HitPoints -= 1;

                    if (nextThingToDamage.DestroyedOrNull())
                    {
                        placedThings.RemoveWhere(x => x == null);
                    }
                }
            }
        }

        #region Placed Things

        public void AddPlacedThing(Thing thing)
        {
            if (!placedThings.Contains(thing))
            {
                placedThings.Add(thing);
            }
        }
        public void RemovePlacedThing(Thing thing)
        {
            if (placedThings.Contains(thing))
            {
                placedThings.Remove(thing);
            }
        }
        public void DestroyAllPlacedThings()
        {
            foreach (var item in placedThings)
            {
                if (item != null && !item.Destroyed)
                {
                    item.Destroy(DestroyMode.Vanish);
                }
            }

            placedThings.Clear();
        }


        public void AddLastStagePlacedThing(Thing thing)
        {
            if (!lastStageThings.Contains(thing))
            {
                lastStageThings.Add(thing);
            }
        }

        public void RemoveLastStagePlacedThing(Thing thing)
        {
            if (lastStageThings.Contains(thing))
            {
                lastStageThings.Remove(thing);
            }
        }

        public void ResetLastStagePlacedThings()
        {
            lastStageThings.Clear();
        }

        public void DestroyLastStagePlacedThings()
        {
            foreach (var item in lastStageThings)
            {
                if (item != null && !item.Destroyed)
                {
                    item.Destroy(DestroyMode.Vanish);
                }
            }

            lastStageThings.Clear();
        }

        #endregion

        #region Drawing and Visualization
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (!showPreview)
                return;

            if (LayoutDef == null || currentStage < 0)
                return;

            List<IntVec3> allPreviewCells = new List<IntVec3>();

            GenDraw.DrawFieldEdges(new List<IntVec3> { Position }, Color.yellow);
            GenDraw.DrawFieldEdges(new List<IntVec3> { LayoutCenter }, Color.magenta);

            DrawStructurePreview(LayoutDef, allPreviewCells);

            if (allPreviewCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(allPreviewCells, Color.cyan);
            }
        }
        private void SetPreviewIndex(int newIndex)
        {
            currentStagePreviewIndex = newIndex;
        }
        private void DrawStructurePreview(StructureLayoutDef layout, List<IntVec3> allPreviewCells)
        {
            //draw them all
            if (currentStagePreviewIndex == -1)
            {
                for (int i = 0; i < layout.stages.Count; i++)
                {
                    int colorIndex = i % previewColors.Count;
                    Color startColor = previewColors[colorIndex];
                    BuildingStage currentBStage = layout.stages[i];

                    List<ThingPlacement> unbuiltWalls = FilterUnbuiltPlacements(currentBStage.walls);
                    List<ThingPlacement> unbuiltDoors = FilterUnbuiltPlacements(currentBStage.doors);
                    List<ThingPlacement> unbuiltPower = FilterUnbuiltPlacements(currentBStage.power);
                    List<ThingPlacement> unbuiltFurniture = FilterUnbuiltPlacements(currentBStage.furniture);
                    List<ThingPlacement> unbuiltOther = FilterUnbuiltPlacements(currentBStage.other);

                    StructurePreviewUtility.DrawThingPreviews(unbuiltWalls, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltDoors, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltPower, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltFurniture, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltOther, Position, Rotation, Map, startColor, allPreviewCells);
                }
            }
            else
            {
                int previewStage = Mathf.Clamp(currentStagePreviewIndex, 0, layout.stages.Count - 1);
                BuildingStage currentBStage = layout.stages[previewStage];

                List<ThingPlacement> unbuiltWalls = FilterUnbuiltPlacements(currentBStage.walls);
                List<ThingPlacement> unbuiltDoors = FilterUnbuiltPlacements(currentBStage.doors);
                List<ThingPlacement> unbuiltPower = FilterUnbuiltPlacements(currentBStage.power);
                List<ThingPlacement> unbuiltFurniture = FilterUnbuiltPlacements(currentBStage.furniture);
                List<ThingPlacement> unbuiltOther = FilterUnbuiltPlacements(currentBStage.other);

                StructurePreviewUtility.DrawThingPreviews(unbuiltWalls, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltDoors, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltPower, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltFurniture, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltOther, Position, Rotation, Map, previewColor, allPreviewCells);
            }
        }
        private List<ThingPlacement> FilterUnbuiltPlacements(List<ThingPlacement> placements)
        {
            List<ThingPlacement> unbuiltPlacements = new List<ThingPlacement>();

            foreach (ThingPlacement placement in placements)
            {
                IntVec3 pos = CalculateCenteredPosition(placement.position);

                if (!pos.InBounds(Map))
                    continue;
                bool isBuilt = false;
                List<Thing> thingsAt = pos.GetThingList(Map);
                foreach (Thing thing in thingsAt)
                {
                    if (thing != this && (thing.def == placement.thing ||
                        (thing.def.entityDefToBuild != null && thing.def.entityDefToBuild == placement.thing)))
                    {
                        isBuilt = true;
                        break;
                    }
                }

                if (!isBuilt)
                {
                    unbuiltPlacements.Add(placement);
                }
            }

            return unbuiltPlacements;
        }
        #endregion

        #region Gizmos and Inspection

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Change stage preview",
                action = () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    for (int i = 0; i < Def.structureLayout.stages.Count; i++)
                    {
                        if (i != currentStagePreviewIndex)
                        {
                            int stageIndex = i;
                            options.Add(new FloatMenuOption($"Stage {i + 1}", () =>
                            {
                                SetPreviewIndex(stageIndex);
                            }));
                        }
                    }

                    if (currentStagePreviewIndex != -1)
                    {
                        options.Add(new FloatMenuOption($"Show all stages", () =>
                        {
                            SetPreviewIndex(-1);
                        }));
                    }

                    options.Add(new FloatMenuOption($"Toggle preview {(showPreview ? "off" : "on")}", () =>
                    {
                        showPreview = !showPreview;
                    }));

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Growth Speed",
                defaultDesc = $"Current ticks between placements: {TicksBetweenBuilds}",
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Haul", true),
                action = () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    float[] speeds = new float[] { 0.1f, 0.5f, 1, 2, 4 };

                    foreach (float speed in speeds)
                    {
                        options.Add(new FloatMenuOption($"{speed} ticks", () =>
                        {
                            GrowthSpeedMultipler = speed;
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };


            // Add gizmo to toggle removing last stage on progress
            yield return new Command_Toggle
            {
                defaultLabel = "Remove previous stage",
                defaultDesc = "If enabled, items from the previous stage will be removed when progressing to the next stage.",
                isActive = () => removeLastStageOnProgress,
                toggleAction = () => removeLastStageOnProgress = !removeLastStageOnProgress
            };

            // Add gizmo to manually remove the last stage
            if (lastStageThings.Count > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Remove last stage items",
                    defaultDesc = "Remove all items that were built in the last stage.",
                    action = () => DestroyLastStagePlacedThings()
                };
            }
        }


        public IntVec3 CalculateCenteredPosition(IntVec2 relativePos)
        {
            return new IntVec3(
                Position.x + relativePos.x,
                Position.y,
                Position.z + relativePos.z
            );
        }


        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(base.GetInspectString()))
            {
                sb.AppendLine(base.GetInspectString());
            }

            sb.AppendLine("Growth Speed Multiplier: " + GrowthSpeedMultipler);

            return sb.ToString().TrimEndNewlines();
        }

        #endregion

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentStage, "currentStage", -1);

            Scribe_Values.Look(ref ticksUntilNextPlacement, "ticksUntilNextPlacement", 0);

            Scribe_Values.Look(ref showPreview, "showPreview", true);
            Scribe_Values.Look(ref currentStagePreviewIndex, "currentStagePreviewIndex", 0);
            Scribe_Values.Look(ref removeLastStageOnProgress, "removeLastStageOnProgress", false);

            Scribe_Values.Look(ref skipFlags, "skipFlags", GrowingSkipFlags.none);
            Scribe_Values.Look(ref includeNaturalTerrain, "includeNaturalTerrain", false);

            Scribe_Defs.Look(ref overrideWallStuff, "overrideWallStuff");
            Scribe_Defs.Look(ref overrideFloorStuff, "overrideFloorStuff");
            Scribe_Defs.Look(ref overrideDoorStuff, "overrideDoorStuff");
            Scribe_Defs.Look(ref overrideFurnitureStuff, "overrideFurnitureStuff");

            Scribe_Collections.Look(ref placedThings, "placedThings", LookMode.Reference);
            Scribe_Collections.Look(ref lastStageThings, "lastStageThings", LookMode.Reference);
        }
    }
}