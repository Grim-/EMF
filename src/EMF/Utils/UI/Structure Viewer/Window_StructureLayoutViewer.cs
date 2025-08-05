using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace EMF
{
    public class Window_StructureLayoutViewer : Window
    {
        private bool isDragging = false;
        private Vector2 dragStartPos;
        private int currentStageIndex = 0;
        private bool showAllStages = false;

        private StructureLayoutDef layoutDef;
        private Building_GrowableStructure growableStructure;

        // Core components
        public LayoutDataManager DataManager { get; private set; }
        public GridRenderer GridRenderer { get; private set; }

        public ToolBar ToolBar { get; private set; }

        public ClipboardManager Clipboard { get; private set; }

        public PaletteBar PaletteBar { get; private set; }

        // UI state
        private Vector2 gridCenter;

        public Vector2 GridCenter
        {
            get => gridCenter;
        }

        private int gridRadius = 20;
        private Rect currentGridRect;

        public Rect CurrentGridRect
        {
            get => currentGridRect;
        }

        public int CurrentStageIndex => currentStageIndex;
        public bool ShowAllStages => showAllStages;

        // Colors
        private Dictionary<int, Color> stageColors = new Dictionary<int, Color>()
        {
            {0, Color.cyan},
            {1, Color.yellow},
            {2, Color.magenta},
            {3, Color.green},
            {4, Color.red},
            {5, Color.blue},
            {6, new Color(1.0f, 0.5f, 0.0f)},
            {7, new Color(0.5f, 0.0f, 0.5f)},
            {8, new Color(0.0f, 0.5f, 0.5f)},
            {9, new Color(1.0f, 0.65f, 0.8f)}
        };

        // Track which multi-cell objects we've already drawn this frame
        private HashSet<PlacementInfo> drawnThisFrame = new HashSet<PlacementInfo>();

        protected LayoutEditorTool currentTool => ToolBar.CurrentTool;

        public LayoutEditorTool GetCurrentTool() => currentTool;
        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public Window_StructureLayoutViewer(StructureLayoutDef layout, Building_GrowableStructure structure = null)
        {
            layoutDef = layout;
            growableStructure = structure;
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            resizeable = true;
            draggable = false;
            gridCenter = new Vector2(550f, 400f);
            Initialize();
        }

        private void Initialize()
        {
            DataManager = new LayoutDataManager(layoutDef);
            DataManager.OnDataChanged += OnDataChanged;
            GridRenderer = new GridRenderer(this);
            ToolBar = new ToolBar(this);
            PaletteBar = new PaletteBar(this);
            Clipboard = new ClipboardManager(this);
            DataManager.ProcessStageData(currentStageIndex, showAllStages);
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawHeader(inRect);
            ToolBar.DrawToolbar(inRect);
            DrawStageControls(inRect);
            float paletteHeight = 44f;
            Rect paletteRect = new Rect(0f, 135f, inRect.width, paletteHeight);
            PaletteBar.Draw(paletteRect);

            currentGridRect = new Rect(0f, 185f, inRect.width, inRect.height - 235f);

            Widgets.DrawWindowBackground(currentGridRect);

            GUI.BeginClip(currentGridRect);

            // Clear the drawn set before starting a new frame
            drawnThisFrame.Clear();

            HandleInput();
            GridRenderer.DrawGrid(currentGridRect, gridRadius, gridCenter);
            GUI.EndClip();

            DrawStatusBar(inRect);
        }


        private void DrawHeader(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), $"Structure Layout: {layoutDef.defName}");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }



        private void DrawStageControls(Rect inRect)
        {
            Rect controlsRect = new Rect(10f, 90f, inRect.width - 20f, 40f);

            // Stage selector button
            Rect stageButtonRect = new Rect(controlsRect.x, controlsRect.y, 150f, 30f);
            if (Widgets.ButtonText(stageButtonRect, showAllStages ? "All Stages" : $"Stage {currentStageIndex + 1}"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int i = 0; i < layoutDef.stages.Count; i++)
                {
                    int stageIndex = i;
                    options.Add(new FloatMenuOption($"Stage {i + 1}", () =>
                    {
                        showAllStages = false;
                        currentStageIndex = stageIndex;
                        DataManager.ProcessStageData(currentStageIndex, showAllStages);
                    }));
                }

                options.Add(new FloatMenuOption("Show All Stages", () =>
                {
                    showAllStages = true;
                    DataManager.ProcessStageData(currentStageIndex, showAllStages);
                }));

                Find.WindowStack.Add(new FloatMenu(options));
            }

            // Zoom controls
            Rect zoomLabel = new Rect(stageButtonRect.xMax + 10f, controlsRect.y + 5f, 100f, 20f);
            Widgets.Label(zoomLabel, $"Zoom: {GridRenderer.CellDrawSize:F0}");

            // Reset button
            Rect resetRect = new Rect(zoomLabel.xMax + 10f, controlsRect.y, 100f, 30f);
            if (Widgets.ButtonText(resetRect, "Reset View"))
            {
                GridRenderer.ResetView();
            }

            // Save/Load buttons
            Rect saveRect = new Rect(resetRect.xMax + 10f, controlsRect.y, 80f, 30f);
            if (Widgets.ButtonText(saveRect, "Save"))
            {
                SaveLayout();
            }

            Rect loadRect = new Rect(saveRect.xMax + 5f, controlsRect.y, 80f, 30f);
            if (Widgets.ButtonText(loadRect, "Load"))
            {
                LoadLayout();
            }
        }

        private void DrawStatusBar(Rect inRect)
        {
            Rect statusRect = new Rect(0f, inRect.height - 40f, inRect.width, 40f);
            Widgets.DrawWindowBackground(statusRect);

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            // Mouse position info
            IntVec2? gridPos = GridRenderer.GetGridPositionFromMouse(Event.current.mousePosition - currentGridRect.position, gridCenter);
            string posInfo = gridPos.HasValue ? $"Grid: [{gridPos.Value.x}, {gridPos.Value.z}]" : "Grid: [--, --]";

            Rect posRect = new Rect(statusRect.x + 10f, statusRect.y, 150f, statusRect.height);
            Widgets.Label(posRect, posInfo);

            // Current tool info
            string toolInfo = $"Tool: {currentTool?.ToolName ?? "None"}";
            Rect toolRect = new Rect(posRect.xMax + 20f, statusRect.y, 150f, statusRect.height);
            Widgets.Label(toolRect, toolInfo);

            // General info
            string info = "LMB: Use tool | RMB: Context menu | Scroll: Zoom | MMB/Shift+LMB: Pan";
            Rect infoRect = new Rect(toolRect.xMax + 20f, statusRect.y, 500f, statusRect.height);
            Widgets.Label(infoRect, info);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        private void HandleInput()
        {
            Event evt = Event.current;

            // Handle keyboard shortcuts
            if (evt.type == EventType.KeyDown)
            {
                if (evt.control)
                {
                    if (evt.keyCode == KeyCode.C)
                    {
                        Clipboard.Copy();
                        evt.Use();
                        return;
                    }
                    else if (evt.keyCode == KeyCode.V)
                    {
                        Clipboard.Paste();
                        evt.Use();
                        return;
                    }
                    else if (evt.keyCode == KeyCode.X)
                    {
                        Clipboard.Cut();
                        evt.Use();
                        return;
                    }
                }
            }

            // Handle panning (middle mouse or shift+left mouse)
            if (evt.button == 3 || (evt.button == 0 && evt.shift))
            {
                GridRenderer.HandlePanning(evt, ref isDragging, ref dragStartPos);
                return;
            }

            // Handle zoom
            GridRenderer.HandleZoom(evt);

            // Get grid position
            IntVec2? gridPos = GridRenderer.GetGridPositionFromMouse(evt.mousePosition, gridCenter);

            if (gridPos.HasValue && Mathf.Abs(gridPos.Value.x) <= gridRadius && Mathf.Abs(gridPos.Value.z) <= gridRadius)
            {
                // Handle right-click context menu
                if (evt.type == EventType.MouseDown && evt.button == 1)
                {
                    ShowContextMenu(gridPos.Value);
                    evt.Use();
                }
                // Handle current tool input
                else if (currentTool != null)
                {
                    currentTool.HandleInput(evt, gridPos.Value);
                }
            }
        }

        public void DrawCellContents(Rect cellRect, IntVec2 gridCoord)
        {
            var contents = DataManager.GetCellContents(gridCoord);

            if (contents.Count > 0)
            {
                // Draw background based on content
                var primary = contents.Last();
                Color bgColor = showAllStages ?
                    GetStageColor(primary.stage) * 0.3f :
                    primary.categoryColor * 0.3f;
                Widgets.DrawBoxSolid(cellRect, bgColor);

                // Draw each placement
                var sortedPlacements = contents.OrderBy(p => GetAltitudeOrder(p)).ToList();
                foreach (var placement in sortedPlacements)
                {
                    DrawPlacement(cellRect, gridCoord, placement);
                }
            }

            // Let current tool draw on top
            currentTool?.Draw(cellRect, gridCoord);

            // Hover tooltip
            if (Mouse.IsOver(cellRect) && contents.Count > 0)
            {
                TooltipHandler.TipRegion(cellRect, GetTooltipForCell(gridCoord));
                Widgets.DrawHighlight(cellRect);
            }
        }

        private void DrawPlacement(Rect cellRect, IntVec2 gridCoord, PlacementInfo placement)
        {
            if (placement.terrainDef != null)
            {
                // Terrain always draws per-cell
                Rect iconRect = cellRect.ContractedBy(cellRect.width * 0.1f);
                Widgets.DefIcon(iconRect, placement.terrainDef);
            }
            else if (placement.thingDef != null)
            {
                // Check if we've already drawn this multi-cell object
                if (drawnThisFrame.Contains(placement))
                    return;

                // For multi-cell objects, calculate the full footprint
                IntVec2 size = placement.thingDef.size;
                if (placement.rotation == Rot4.East || placement.rotation == Rot4.West)
                {
                    size = new IntVec2(size.z, size.x);
                }

                // Only draw if this is the "origin" cell (bottom-left of the footprint)
                // Assuming placement stores the origin position
                IntVec2 originPos = GetOriginPosition(placement, gridCoord);
                if (gridCoord != originPos)
                    return;

                // Mark as drawn
                drawnThisFrame.Add(placement);

                // Calculate the full rect covering all cells
                float cellSize = GridRenderer.CellDrawSize;
                Rect fullRect = new Rect(
                    cellRect.x,
                    cellRect.y - (size.z - 1) * cellSize, // Adjust for RimWorld's coordinate system
                    size.x * cellSize,
                    size.z * cellSize
                );
                fullRect = fullRect.ContractedBy(cellSize * 0.1f);

                Texture2D texture = placement.stuffDef != null ?
                    placement.thingDef.GetUIIconForStuff(placement.stuffDef) :
                    placement.thingDef.uiIcon ?? BaseContent.BadTex;

                if (placement.rotation != Rot4.North && placement.rotation != Rot4.Invalid)
                {
                    Matrix4x4 matrix = GUI.matrix;
                    GUIUtility.RotateAroundPivot(placement.rotation.AsAngle, fullRect.center);
                    GUI.DrawTexture(fullRect, texture);
                    GUI.matrix = matrix;
                }
                else
                {
                    GUI.DrawTexture(fullRect, texture);
                }
            }
        }

        private IntVec2 GetOriginPosition(PlacementInfo placement, IntVec2 currentPos)
        {
            // This method needs to determine the origin position of a multi-cell object
            // The implementation depends on how your PlacementInfo stores position data
            // If PlacementInfo stores the origin position, you might need to track that
            // For now, assuming the placement is stored at its origin position

            // You'll need to implement this based on your data structure
            // This is a placeholder that assumes we can query the DataManager
            // for the origin position of this placement

            // Option 1: If PlacementInfo has an origin position field
            // return placement.originPosition;

            // Option 2: Search for the bottom-left cell that contains this placement
            if (placement.thingDef == null || placement.thingDef.size == IntVec2.One)
                return currentPos;

            IntVec2 size = placement.thingDef.size;
            if (placement.rotation == Rot4.East || placement.rotation == Rot4.West)
            {
                size = new IntVec2(size.z, size.x);
            }

            // Search for the origin (bottom-left in RimWorld coordinates)
            for (int x = currentPos.x; x >= currentPos.x - size.x + 1; x--)
            {
                for (int z = currentPos.z; z >= currentPos.z - size.z + 1; z--)
                {
                    IntVec2 checkPos = new IntVec2(x, z);
                    var contents = DataManager.GetCellContents(checkPos);
                    if (contents.Contains(placement))
                    {
                        // Check if this could be the origin
                        bool isOrigin = true;
                        for (int dx = 0; dx < size.x && isOrigin; dx++)
                        {
                            for (int dz = 0; dz < size.z && isOrigin; dz++)
                            {
                                IntVec2 cellPos = new IntVec2(x + dx, z + dz);
                                var cellContents = DataManager.GetCellContents(cellPos);
                                if (!cellContents.Contains(placement))
                                {
                                    isOrigin = false;
                                }
                            }
                        }
                        if (isOrigin)
                            return checkPos;
                    }
                }
            }

            return currentPos; // Fallback
        }

        private Rect CalculateThingRect(Rect cellRect, PlacementInfo placement)
        {
            IntVec2 baseSize = placement.thingDef.size;
            float cellSize = GridRenderer.CellDrawSize;

            if (placement.rotation == Rot4.East || placement.rotation == Rot4.West)
            {
                return new Rect(cellRect.x, cellRect.y, baseSize.z * cellSize, baseSize.x * cellSize);
            }
            return new Rect(cellRect.x, cellRect.y, baseSize.x * cellSize, baseSize.z * cellSize);
        }

        private void ShowContextMenu(IntVec2 position)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            var contents = DataManager.GetCellContents(position);

            if (contents.Count > 0)
            {
                // Edit/remove options for existing content
                foreach (var placement in contents)
                {
                    string label = placement.terrainDef != null ?
                        $"Remove {placement.terrainDef.LabelCap}" :
                        $"Remove {placement.thingDef.LabelCap}";

                    options.Add(new FloatMenuOption(label, () => DataManager.RemovePlacement(position, placement)));

                    if (placement.thingDef != null)
                    {
                        options.Add(new FloatMenuOption($"Rotate {placement.thingDef.LabelCap}",
                            () => DataManager.RotatePlacement(position, placement)));
                    }
                }

                options.Add(new FloatMenuOption("Clear All", () => DataManager.RemoveAllAtPosition(position)));
                options.Add(new FloatMenuOption("------------------------", null));
            }

            // Add new content options
            options.Add(new FloatMenuOption("Add Terrain...", () => ShowTerrainMenu(position)));
            options.Add(new FloatMenuOption("Add Wall...", () => ShowThingMenu(position, "Wall")));
            options.Add(new FloatMenuOption("Add Door...", () => ShowThingMenu(position, "Door")));
            options.Add(new FloatMenuOption("Add Power...", () => ShowThingMenu(position, "Power")));
            options.Add(new FloatMenuOption("Add Furniture...", () => ShowThingMenu(position, "Furniture")));
            options.Add(new FloatMenuOption("Add Other...", () => ShowThingMenu(position, "Other")));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowTerrainMenu(IntVec2 position)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            var terrains = DefDatabase<TerrainDef>.AllDefsListForReading
                .Where(t => t.designationCategory != null)
                .OrderBy(t => t.label);

            foreach (var terrain in terrains)
            {
                options.Add(new FloatMenuOption(terrain.LabelCap, () =>
                {
                    var item = new PaintableItem(terrain);
                    ApplyPaintItem(position, item);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowThingMenu(IntVec2 position, string category)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            var things = GetThingsForCategory(category).OrderBy(t => t.label);

            foreach (var thing in things)
            {
                if (thing.MadeFromStuff)
                {
                    options.Add(new FloatMenuOption(thing.LabelCap + "...",
                        () => ShowStuffMenu(position, thing, category)));
                }
                else
                {
                    options.Add(new FloatMenuOption(thing.LabelCap, () =>
                    {
                        var item = new PaintableItem(thing, null, category);
                        ApplyPaintItem(position, item);
                    }));
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowStuffMenu(IntVec2 position, ThingDef thingDef, string category)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            var stuffs = GenStuff.AllowedStuffsFor(thingDef).OrderBy(s => s.label);

            foreach (var stuff in stuffs)
            {
                options.Add(new FloatMenuOption(stuff.LabelCap, () =>
                {
                    var item = new PaintableItem(thingDef, stuff, category);
                    ApplyPaintItem(position, item);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public IEnumerable<ThingDef> GetThingsForCategory(string category)
        {
            switch (category)
            {
                case "Wall":
                    return DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(t => t.designationCategory != null &&
                               (t.defName.Contains("Wall") || t.building?.isPlaceOverableWall == true));

                case "Door":
                    return DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(t => t.thingClass == typeof(Building_Door) ||
                               t.thingClass.IsSubclassOf(typeof(Building_Door)));

                case "Power":
                    return DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(t => t.HasComp(typeof(CompPowerTrader)) ||
                               t.HasComp(typeof(CompPowerBattery)) ||
                               t.HasComp(typeof(CompPowerTransmitter)));

                case "Furniture":
                    return DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(t => t.designationCategory != null &&
                               t.category == ThingCategory.Building &&
                               !t.HasComp(typeof(CompPowerTrader)) &&
                               t.thingClass != typeof(Building_Door) &&
                               !t.defName.Contains("Wall"));

                default:
                    return DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(t => t.designationCategory != null &&
                               t.category == ThingCategory.Building);
            }
        }

        public void ApplyPaintItem(IntVec2 position, PaintableItem item)
        {
            var placement = new PlacementInfo
            {
                category = item.Category,
                thingDef = item.ThingDef,
                terrainDef = item.TerrainDef,
                stuffDef = item.StuffDef,
                rotation = item.Rotation,
                stage = showAllStages ? 0 : currentStageIndex,
                categoryColor = GetCategoryColor(item.Category)
            };

            DataManager.AddPlacement(position, placement);
        }

        public void RemoveAllAtPosition(IntVec2 position)
        {
            DataManager.RemoveAllAtPosition(position);
        }

        public Color GetCategoryColor(string category)
        {
            switch (category)
            {
                case "Terrain": return new Color(0.5f, 0.4f, 0.3f, 0.8f);
                case "Wall": return new Color(0.7f, 0.7f, 0.7f, 0.8f);
                case "Door": return new Color(0.6f, 0.4f, 0.2f, 0.8f);
                case "Power": return new Color(1f, 1f, 0f, 0.8f);
                case "Furniture": return new Color(0.4f, 0.6f, 0.8f, 0.8f);
                default: return new Color(0.8f, 0.4f, 0.8f, 0.8f);
            }
        }

        public Color GetStageColor(int stage)
        {
            return stageColors.TryGetValue(stage, Color.gray);
        }

        private int GetAltitudeOrder(PlacementInfo placement)
        {
            if (placement.terrainDef != null)
                return 0;

            if (placement.thingDef == null)
                return 1;

            return (int)placement.thingDef.altitudeLayer;
        }

        private string GetTooltipForCell(IntVec2 position)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Position: [{position.x}, {position.z}]");

            var contents = DataManager.GetCellContents(position);
            if (contents.Count > 0)
            {
                sb.AppendLine();
                foreach (var placement in contents)
                {
                    sb.AppendLine($"Stage {placement.stage + 1}:");
                    if (placement.thingDef != null)
                    {
                        sb.AppendLine($"  {placement.thingDef.LabelCap}");
                        if (placement.stuffDef != null)
                        {
                            sb.AppendLine($"  Material: {placement.stuffDef.LabelCap}");
                        }
                    }
                    if (placement.terrainDef != null)
                    {
                        sb.AppendLine($"  {placement.terrainDef.LabelCap}");
                    }
                }
            }

            return sb.ToString().TrimEndNewlines();
        }

        private void SaveLayout()
        {
            // Save the current layout to def
            // dataManager.SaveToLayoutDef();
            Messages.Message("Layout saved", MessageTypeDefOf.PositiveEvent);
        }

        private void LoadLayout()
        {
            // Reload from original def
            // dataManager.ReloadFromDef();
            Messages.Message("Layout reloaded", MessageTypeDefOf.NeutralEvent);
        }

        private void OnDataChanged()
        {
            // Any additional handling when data changes
        }
    }
}