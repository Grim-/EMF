using UnityEngine;
using Verse;

namespace EMF
{
    public class PaintableItem
    {
        public string Label { get; }
        public string Category { get; }
        public ThingDef ThingDef { get; }
        public TerrainDef TerrainDef { get; }
        public ThingDef StuffDef { get; }
        public Rot4 Rotation { get; set; }

        public Texture2D PreviewTexture
        {
            get
            {
                if (TerrainDef != null) return TerrainDef.uiIcon;
                if (ThingDef != null)
                {
                    return StuffDef != null ?
                        ThingDef.GetUIIconForStuff(StuffDef) :
                        ThingDef.uiIcon;
                }
                return BaseContent.BadTex;
            }
        }

        public PaintableItem(TerrainDef terrain)
        {
            TerrainDef = terrain;
            Label = terrain.LabelCap;
            Category = "Terrain";
        }

        public PaintableItem(ThingDef thing, ThingDef stuff, string category)
        {
            ThingDef = thing;
            StuffDef = stuff;
            Category = category;
            Label = thing.LabelCap;
            Rotation = Rot4.North;
        }
    }


    //public class Window_StructureLayoutViewer : Window
    //{
    //    private StructureLayoutDef layoutDef;
    //    private Building_GrowableStructure growableStructure;

    //    private float cellSize = 40f;
    //    protected float CellDrawSize => cellSize * zoomFactor;

    //    private Vector2 gridCenter;
    //    private int gridRadius = 20;
    //    private Rect currentGridRect;
    //    private Vector2 scrollPosition = Vector2.zero;
    //    private int currentStageIndex = 0;
    //    private bool showAllStages = false;
    //    private Vector2 dragStartPos;
    //    private Vector2 currentGridOffset = Vector2.zero;
    //    private bool isDragging = false;

    //    protected float zoomFactor = 1f;
    //    private float zoomSpeed = 0.03f;
    //    private Dictionary<IntVec2, List<PlacementInfo>> cellContents = new Dictionary<IntVec2, List<PlacementInfo>>();

    //    private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
    //    private Color gridFarColor = new Color(1f, 1f, 1f, 0.1f);
    //    private Color terrainColor = new Color(0.5f, 0.4f, 0.3f, 0.8f);
    //    private Color wallColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    //    private Color doorColor = new Color(0.6f, 0.4f, 0.2f, 0.8f);
    //    private Color powerColor = new Color(1f, 1f, 0f, 0.8f);
    //    private Color furnitureColor = new Color(0.4f, 0.6f, 0.8f, 0.8f);
    //    private Color otherColor = new Color(0.8f, 0.4f, 0.8f, 0.8f);

    //    private Dictionary<int, Color> stageColors = new Dictionary<int, Color>()
    //    {
    //        {0, Color.cyan},
    //        {1, Color.yellow},
    //        {2, Color.magenta},
    //        {3, Color.green},
    //        {4, Color.red},
    //        {5, Color.blue},
    //        {6, new Color(1.0f, 0.5f, 0.0f)},
    //        {7, new Color(0.5f, 0.0f, 0.5f)},
    //        {8, new Color(0.0f, 0.5f, 0.5f)},
    //        {9, new Color(1.0f, 0.65f, 0.8f)}
    //    };

    //    public override Vector2 InitialSize => new Vector2(900f, 700f);

    //    private class PlacementInfo
    //    {
    //        public string category;
    //        public ThingDef thingDef;
    //        public TerrainDef terrainDef;
    //        public ThingDef stuffDef;
    //        public Rot4 rotation;
    //        public int stage;
    //        public Color categoryColor;
    //    }

    //    public Window_StructureLayoutViewer(StructureLayoutDef layout, Building_GrowableStructure structure = null)
    //    {
    //        layoutDef = layout.DeepCopy();
    //        growableStructure = structure;
    //        forcePause = true;
    //        doCloseX = true;
    //        absorbInputAroundWindow = true;
    //        closeOnClickedOutside = false;
    //        gridCenter = new Vector2(450f, 300f);
    //        ProcessLayoutData();
    //    }

    //    private void ProcessLayoutData()
    //    {
    //        cellContents.Clear();

    //        if (showAllStages)
    //        {
    //            for (int i = 0; i < layoutDef.stages.Count; i++)
    //            {
    //                ProcessStage(i);
    //            }
    //        }
    //        else
    //        {
    //            ProcessStage(currentStageIndex);
    //        }
    //    }

    //    private void ProcessStage(int stageIndex)
    //    {
    //        if (stageIndex < 0 || stageIndex >= layoutDef.stages.Count) return;

    //        BuildingStage stage = layoutDef.stages[stageIndex];

    //        foreach (var terrain in stage.terrain)
    //        {
    //            AddPlacement(terrain.position, new PlacementInfo
    //            {
    //                category = "Terrain",
    //                terrainDef = terrain.terrain,
    //                stage = stageIndex,
    //                categoryColor = terrainColor
    //            });
    //        }

    //        foreach (var wall in stage.walls)
    //        {
    //            AddPlacement(wall.position, new PlacementInfo
    //            {
    //                category = "Wall",
    //                thingDef = wall.thing,
    //                stuffDef = wall.stuff,
    //                rotation = wall.rotation,
    //                stage = stageIndex,
    //                categoryColor = wallColor
    //            });
    //        }

    //        foreach (var door in stage.doors)
    //        {
    //            AddPlacement(door.position, new PlacementInfo
    //            {
    //                category = "Door",
    //                thingDef = door.thing,
    //                stuffDef = door.stuff,
    //                rotation = door.rotation,
    //                stage = stageIndex,
    //                categoryColor = doorColor
    //            });
    //        }

    //        foreach (var power in stage.power)
    //        {
    //            AddPlacement(power.position, new PlacementInfo
    //            {
    //                category = "Power",
    //                thingDef = power.thing,
    //                stuffDef = power.stuff,
    //                rotation = power.rotation,
    //                stage = stageIndex,
    //                categoryColor = powerColor
    //            });
    //        }

    //        foreach (var furniture in stage.furniture)
    //        {
    //            AddPlacement(furniture.position, new PlacementInfo
    //            {
    //                category = "Furniture",
    //                thingDef = furniture.thing,
    //                stuffDef = furniture.stuff,
    //                rotation = furniture.rotation,
    //                stage = stageIndex,
    //                categoryColor = furnitureColor
    //            });
    //        }

    //        foreach (var other in stage.other)
    //        {
    //            AddPlacement(other.position, new PlacementInfo
    //            {
    //                category = "Other",
    //                thingDef = other.thing,
    //                stuffDef = other.stuff,
    //                rotation = other.rotation,
    //                stage = stageIndex,
    //                categoryColor = otherColor
    //            });
    //        }
    //    }

    //    private void AddPlacement(IntVec2 position, PlacementInfo info)
    //    {
    //        if (!cellContents.ContainsKey(position))
    //        {
    //            cellContents[position] = new List<PlacementInfo>();
    //        }
    //        cellContents[position].Add(info);
    //    }

    //    public override void DoWindowContents(Rect inRect)
    //    {
    //        Text.Font = GameFont.Medium;
    //        Text.Anchor = TextAnchor.MiddleCenter;
    //        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), $"Structure Layout: {layoutDef.defName}");
    //        Text.Anchor = TextAnchor.UpperLeft;

    //        DrawControls(inRect);

    //        currentGridRect = new Rect(0f, 100f, inRect.width, inRect.height - 150f);
    //        DrawLayoutGrid(currentGridRect);

    //        DrawToolsPanel(inRect);
    //    }

    //    private void DrawToolsPanel(Rect inRect)
    //    {
    //        float buttonSize = 32f;
    //        float spacing = 4f;
    //        int buttonCount = 4;

    //        float panelWidth = (buttonSize * buttonCount) + (spacing * (buttonCount + 1));
    //        float panelHeight = buttonSize + (spacing * 2);
    //        Rect panelRect = new Rect(10f, inRect.height - panelHeight - 10f, panelWidth, panelHeight);

    //        Widgets.DrawWindowBackground(panelRect);

    //        Rect currentButtonRect = new Rect(panelRect.x + spacing, panelRect.y + spacing, buttonSize, buttonSize);

    //        currentButtonRect.x += buttonSize + spacing;

    //        if (Widgets.ButtonImage(currentButtonRect, TexButton.Delete))
    //        {

    //        }
    //        TooltipHandler.TipRegion(currentButtonRect, "Delete");
    //        currentButtonRect.x += buttonSize + spacing;

    //        if (Widgets.ButtonImage(currentButtonRect, TexButton.Copy))
    //        {

    //        }
    //        TooltipHandler.TipRegion(currentButtonRect, "Copy");
    //        currentButtonRect.x += buttonSize + spacing;

    //        if (Widgets.ButtonImage(currentButtonRect, TexButton.Paste))
    //        {

    //        }
    //        TooltipHandler.TipRegion(currentButtonRect, "Paste");
    //    }

    //    private void DrawControls(Rect inRect)
    //    {
    //        Rect controlsRect = new Rect(10f, 40f, inRect.width - 20f, 50f);

    //        Rect stageButtonRect = new Rect(controlsRect.x, controlsRect.y, 150f, 30f);
    //        if (Widgets.ButtonText(stageButtonRect, showAllStages ? "All Stages" : $"Stage {currentStageIndex + 1}"))
    //        {
    //            List<FloatMenuOption> options = new List<FloatMenuOption>();

    //            for (int i = 0; i < layoutDef.stages.Count; i++)
    //            {
    //                int stageIndex = i;
    //                options.Add(new FloatMenuOption($"Stage {i + 1}", () =>
    //                {
    //                    showAllStages = false;
    //                    currentStageIndex = stageIndex;
    //                    ProcessLayoutData();
    //                }));
    //            }

    //            options.Add(new FloatMenuOption("Show All Stages", () =>
    //            {
    //                showAllStages = true;
    //                ProcessLayoutData();
    //            }));

    //            Find.WindowStack.Add(new FloatMenu(options));
    //        }

    //        Rect zoomRect = new Rect(stageButtonRect.xMax + 10f, controlsRect.y, 200f, 30f);
    //        Rect zoomLabel = new Rect(zoomRect.xMax + 5f, controlsRect.y, 100f, 30f);
    //        Widgets.Label(zoomLabel, $"Zoom: {CellDrawSize:F0}");

    //        Rect resetRect = new Rect(zoomLabel.xMax + 10f, controlsRect.y, 100f, 30f);
    //        if (Widgets.ButtonText(resetRect, "Reset View"))
    //        {
    //            currentGridOffset = Vector2.zero;
    //            zoomFactor = 1f;
    //        }

    //        Text.Font = GameFont.Tiny;
    //        Rect infoRect = new Rect(controlsRect.x, controlsRect.y + 35f, inRect.width - 20f, 20f);
    //        Widgets.Label(infoRect, "Left-click and drag to pan. Hover over cells to see contents.");
    //        Text.Font = GameFont.Small;
    //    }

    //    private void DrawLayoutGrid(Rect rect)
    //    {
    //        GUI.BeginClip(rect);

    //        HandleCanvasInput(rect);

    //        Vector2 adjustedCenter = gridCenter + currentGridOffset;
    //        Event evt = Event.current;

    //        for (int x = -gridRadius; x <= gridRadius; x++)
    //        {
    //            for (int z = -gridRadius; z <= gridRadius; z++)
    //            {
    //                Vector2 cellPos = adjustedCenter + new Vector2(x * CellDrawSize, -z * CellDrawSize);
    //                Rect cellRect = new Rect(cellPos.x - CellDrawSize / 2f, cellPos.y - CellDrawSize / 2f, CellDrawSize, CellDrawSize);

    //                if (!cellRect.Overlaps(new Rect(0, 0, rect.width, rect.height)))
    //                    continue;

    //                float t = Mathf.Clamp01(Vector2.Distance(cellPos, adjustedCenter) / (gridRadius * CellDrawSize));
    //                Widgets.DrawBoxSolidWithOutline(cellRect, Color.clear, Color.Lerp(gridColor, gridFarColor, t));

    //                IntVec2 gridCoord = new IntVec2(x, z);

    //                if (cellContents.ContainsKey(gridCoord))
    //                {
    //                    DrawCellContents(cellRect, gridCoord);
    //                }

    //                if (evt.type == EventType.MouseDown && evt.button == 1 && cellRect.Contains(evt.mousePosition))
    //                {
    //                    if (cellContents.ContainsKey(gridCoord) && cellContents[gridCoord].Any())
    //                    {
    //                        ShowEditMenu(gridCoord);
    //                    }
    //                    else
    //                    {
    //                        ShowCreationMenu(gridCoord);
    //                    }
    //                    evt.Use();
    //                }
    //            }
    //        }

    //        Rect centerMarker = new Rect(adjustedCenter.x - 5f, adjustedCenter.y - 5f, 10f, 10f);
    //        Widgets.DrawBoxSolid(centerMarker, Color.red);

    //        GUI.EndClip();
    //    }
    //    private void HandleCanvasInput(Rect gridRect)
    //    {
    //        Event evt = Event.current;

    //        if (evt.type == EventType.MouseDown && evt.button == 0 && gridRect.Contains(evt.mousePosition))
    //        {
    //            isDragging = true;
    //            dragStartPos = evt.mousePosition;
    //            evt.Use();
    //        }
    //        else if (evt.type == EventType.MouseDrag && isDragging)
    //        {
    //            Vector2 delta = evt.mousePosition - dragStartPos;
    //            currentGridOffset += delta;
    //            dragStartPos = evt.mousePosition;
    //            evt.Use();
    //        }
    //        else if (evt.type == EventType.MouseUp && isDragging)
    //        {
    //            isDragging = false;
    //            evt.Use();
    //        }
    //        else if (evt.type == EventType.ScrollWheel && gridRect.Contains(evt.mousePosition))
    //        {
    //            float newZoomFactor = zoomFactor - evt.delta.y * zoomSpeed;
    //            zoomFactor = Mathf.Clamp(newZoomFactor, 0.1f, 10f);
    //            evt.Use();
    //        }
    //    }

    //    private void ShowCreationMenu(IntVec2 position)
    //    {
    //        List<FloatMenuOption> options = new List<FloatMenuOption>();

    //        options.Add(new FloatMenuOption("Terrain", () => ShowTerrainMenu(position)));
    //        options.Add(new FloatMenuOption("Wall", () => ShowThingMenu(position, "Wall", wallColor)));
    //        options.Add(new FloatMenuOption("Door", () => ShowThingMenu(position, "Door", doorColor)));
    //        options.Add(new FloatMenuOption("Power", () => ShowThingMenu(position, "Power", powerColor)));
    //        options.Add(new FloatMenuOption("Furniture", () => ShowThingMenu(position, "Furniture", furnitureColor)));
    //        options.Add(new FloatMenuOption("Other", () => ShowThingMenu(position, "Other", otherColor)));

    //        Find.WindowStack.Add(new FloatMenu(options));
    //    }

    //    private void ShowEditMenu(IntVec2 position)
    //    {
    //        List<FloatMenuOption> options = new List<FloatMenuOption>();
    //        var placements = cellContents[position];

    //        foreach (var placement in placements)
    //        {
    //            string label = placement.terrainDef != null ?
    //                $"Remove {placement.terrainDef.LabelCap}" :
    //                $"Remove {placement.thingDef.LabelCap}";

    //            options.Add(new FloatMenuOption(label, () => RemovePlacement(position, placement)));

    //            if (placement.thingDef != null)
    //            {
    //                options.Add(new FloatMenuOption($"Rotate {placement.thingDef.LabelCap}", () => RotatePlacement(position, placement)));
    //            }
    //        }

    //        options.Add(new FloatMenuOption("Add New...", () => ShowCreationMenu(position)));

    //        Find.WindowStack.Add(new FloatMenu(options));
    //    }

    //    private void ShowTerrainMenu(IntVec2 position)
    //    {
    //        List<FloatMenuOption> options = new List<FloatMenuOption>();

    //        var terrains = DefDatabase<TerrainDef>.AllDefsListForReading
    //            .Where(t => t.designationCategory != null)
    //            .OrderBy(t => t.label);

    //        foreach (var terrain in terrains)
    //        {
    //            options.Add(new FloatMenuOption(terrain.LabelCap, () => AddTerrainPlacement(position, terrain)));
    //        }

    //        Find.WindowStack.Add(new FloatMenu(options));
    //    }

    //    private void ShowThingMenu(IntVec2 position, string category, Color categoryColor)
    //    {
    //        List<FloatMenuOption> options = new List<FloatMenuOption>();

    //        var things = GetThingsForCategory(category).OrderBy(t => t.label);

    //        foreach (var thing in things)
    //        {
    //            if (thing.MadeFromStuff)
    //            {
    //                options.Add(new FloatMenuOption(thing.LabelCap + "...", () => ShowStuffMenu(position, thing, category, categoryColor)));
    //            }
    //            else
    //            {
    //                options.Add(new FloatMenuOption(thing.LabelCap, () => AddThingPlacement(position, thing, null, category, categoryColor)));
    //            }
    //        }

    //        Find.WindowStack.Add(new FloatMenu(options));
    //    }

    //    private void ShowStuffMenu(IntVec2 position, ThingDef thingDef, string category, Color categoryColor)
    //    {
    //        List<FloatMenuOption> options = new List<FloatMenuOption>();

    //        var stuffs = GenStuff.AllowedStuffsFor(thingDef).OrderBy(s => s.label);

    //        foreach (var stuff in stuffs)
    //        {
    //            options.Add(new FloatMenuOption(stuff.LabelCap, () => AddThingPlacement(position, thingDef, stuff, category, categoryColor)));
    //        }

    //        Find.WindowStack.Add(new FloatMenu(options));
    //    }

    //    private IEnumerable<ThingDef> GetThingsForCategory(string category)
    //    {
    //        switch (category)
    //        {
    //            case "Wall":
    //                return DefDatabase<ThingDef>.AllDefsListForReading
    //                    .Where(t => t.designationCategory != null &&
    //                           (t.defName.Contains("Wall") || t.building?.isPlaceOverableWall == true));

    //            case "Door":
    //                return DefDatabase<ThingDef>.AllDefsListForReading
    //                    .Where(t => t.thingClass == typeof(Building_Door) || t.thingClass.IsSubclassOf(typeof(Building_Door)));

    //            case "Power":
    //                return DefDatabase<ThingDef>.AllDefsListForReading
    //                    .Where(t => t.HasComp(typeof(CompPowerTrader)) || t.HasComp(typeof(CompPowerBattery)) ||
    //                           t.HasComp(typeof(CompPowerTransmitter)));

    //            case "Furniture":
    //                return DefDatabase<ThingDef>.AllDefsListForReading
    //                    .Where(t => t.designationCategory != null && t.category == ThingCategory.Building &&
    //                           !t.HasComp(typeof(CompPowerTrader)) && t.thingClass != typeof(Building_Door) &&
    //                           !t.defName.Contains("Wall"));

    //            default:
    //                return DefDatabase<ThingDef>.AllDefsListForReading
    //                    .Where(t => t.designationCategory != null && t.category == ThingCategory.Building);
    //        }
    //    }

    //    private void AddTerrainPlacement(IntVec2 position, TerrainDef terrain)
    //    {
    //        var placement = new PlacementInfo
    //        {
    //            category = "Terrain",
    //            terrainDef = terrain,
    //            stage = showAllStages ? 0 : currentStageIndex,
    //            categoryColor = terrainColor
    //        };

    //        AddPlacement(position, placement);
    //        UpdateLayoutDef();
    //    }

    //    private void AddThingPlacement(IntVec2 position, ThingDef thing, ThingDef stuff, string category, Color categoryColor)
    //    {
    //        var placement = new PlacementInfo
    //        {
    //            category = category,
    //            thingDef = thing,
    //            stuffDef = stuff,
    //            rotation = Rot4.North,
    //            stage = showAllStages ? 0 : currentStageIndex,
    //            categoryColor = categoryColor
    //        };

    //        AddPlacement(position, placement);
    //        UpdateLayoutDef();
    //    }

    //    private void RemovePlacement(IntVec2 position, PlacementInfo placement)
    //    {
    //        if (cellContents.ContainsKey(position))
    //        {
    //            cellContents[position].Remove(placement);
    //            if (cellContents[position].Count == 0)
    //            {
    //                cellContents.Remove(position);
    //            }
    //            UpdateLayoutDef();
    //        }
    //    }

    //    private void RotatePlacement(IntVec2 position, PlacementInfo placement)
    //    {
    //        placement.rotation.Rotate(RotationDirection.Clockwise);
    //        UpdateLayoutDef();
    //    }

    //    private void UpdateLayoutDef()
    //    {
    //    }
    //    private void DrawCellContents(Rect cellRect, IntVec2 gridCoord)
    //    {
    //        List<PlacementInfo> placements = cellContents[gridCoord];
    //        if (placements.Count == 0)
    //            return;

    //        var sortedPlacements = placements.OrderBy(p => GetAltitudeOrder(p)).ToList();

    //        DrawCellBackground(cellRect, sortedPlacements.Last());
    //        foreach (var placement in sortedPlacements)
    //        {
    //            DrawPlacement(cellRect, gridCoord, placement);
    //        }
    //    }

    //    private void DrawCellBackground(Rect cellRect, PlacementInfo primary)
    //    {
    //        Color bgColor = showAllStages ?
    //            stageColors.TryGetValue(primary.stage, Color.gray) * 0.3f :
    //            primary.categoryColor * 0.3f;
    //        Widgets.DrawBoxSolid(cellRect, bgColor);
    //    }

    //    private void DrawPlacement(Rect cellRect, IntVec2 gridCoord, PlacementInfo placement)
    //    {
    //        if (placement.terrainDef != null)
    //        {
    //            DrawTerrain(cellRect, gridCoord, placement);
    //        }

    //        if (placement.thingDef != null)
    //        {
    //            DrawThing(cellRect, gridCoord, placement);
    //        }
    //    }

    //    private void DrawTerrain(Rect cellRect, IntVec2 gridCoord, PlacementInfo placement)
    //    {
    //        Rect iconRect = cellRect.ContractedBy(CellDrawSize * 0.1f);
    //        Widgets.DefIcon(iconRect, placement.terrainDef);
    //    }

    //    private void DrawThing(Rect cellRect, IntVec2 gridCoord, PlacementInfo placement)
    //    {
    //        Rect footprintRect = CalculateThingRect(cellRect, placement);
    //        footprintRect = footprintRect.ContractedBy(CellDrawSize * 0.1f);

    //        if (Mouse.IsOver(footprintRect))
    //        {
    //            TooltipHandler.TipRegion(footprintRect, GetTooltipForCell(gridCoord, placement));
    //            Widgets.DrawHighlight(footprintRect);
    //        }

    //        Texture2D texture = placement.stuffDef != null ? placement.thingDef.GetUIIconForStuff(placement.stuffDef) : placement.thingDef.uiIcon ?? BaseContent.BadTex;

    //        bool isRotated = placement.rotation != Rot4.North && placement.rotation != Rot4.Invalid;

    //        if (isRotated)
    //        {
    //            Vector2 unrotatedSize = new Vector2(placement.thingDef.size.x * CellDrawSize, placement.thingDef.size.z * CellDrawSize);
    //            float contractAmount = CellDrawSize * 0.05f * 2;
    //            Rect drawRect = new Rect(0, 0, unrotatedSize.x - contractAmount, unrotatedSize.y - contractAmount);
    //            drawRect.center = footprintRect.center;

    //            DrawRotatedTexture(drawRect, texture, placement.rotation);
    //        }
    //        else
    //        {
    //            GUI.DrawTexture(footprintRect, texture);
    //        }
    //    }

    //    private Rect CalculateThingRect(Rect cellRect, PlacementInfo placement)
    //    {
    //        IntVec2 baseSize = placement.thingDef.size;
    //        var dimensions = GetRotatedDimensions(baseSize, placement.rotation);
    //        var offset = GetRotationOffset(baseSize, placement.rotation);

    //        return new Rect(
    //            cellRect.x + offset.x,
    //            cellRect.y + offset.y,
    //            dimensions.x,
    //            dimensions.y
    //        );
    //    }

    //    private Vector2 GetRotatedDimensions(IntVec2 baseSize, Rot4 rotation)
    //    {
    //        if (rotation == Rot4.East || rotation == Rot4.West)
    //        {
    //            return new Vector2(baseSize.z * CellDrawSize, baseSize.x * CellDrawSize);
    //        }
    //        return new Vector2(baseSize.x * CellDrawSize, baseSize.z * CellDrawSize);
    //    }

    //    private Vector2 GetRotationOffset(IntVec2 baseSize, Rot4 rotation)
    //    {
    //        float displayWidth = baseSize.x * CellDrawSize;
    //        float displayHeight = baseSize.z * CellDrawSize;

    //        if (rotation == Rot4.East || rotation == Rot4.West)
    //        {
    //            // Swap dimensions for sideways rotations
    //            displayWidth = baseSize.z * CellDrawSize;
    //            displayHeight = baseSize.x * CellDrawSize;
    //        }

    //        switch (rotation.AsInt)
    //        {
    //            case 0: // North
    //                return new Vector2(0, CellDrawSize - displayHeight);

    //            case 1: // East
    //                return new Vector2(0, CellDrawSize - displayHeight);

    //            case 2: // South
    //                return new Vector2(CellDrawSize - displayWidth, 0);

    //            case 3: // West
    //                return new Vector2(CellDrawSize - displayWidth, 0);

    //            default:
    //                return Vector2.zero;
    //        }
    //    }

    //    private void DrawRotatedTexture(Rect rect, Texture2D texture, Rot4 rotation)
    //    {
    //        Matrix4x4 matrix = GUI.matrix;
    //        GUIUtility.RotateAroundPivot(rotation.AsAngle, rect.center);
    //        GUI.DrawTexture(rect, texture);
    //        GUI.matrix = matrix;
    //    }

    //    private int GetAltitudeOrder(PlacementInfo placement)
    //    {
    //        if (placement.terrainDef != null)
    //            return 0;

    //        if (placement.thingDef == null)
    //            return 1;
    //        return (int)placement.thingDef.altitudeLayer;
    //    }

    //    private string GetTooltipForCell(IntVec2 position, PlacementInfo placement)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.AppendLine($"[{position.x}, {position.z}]");

    //        sb.AppendLine();
    //        sb.AppendLine($"Stage {placement.stage + 1}");
    //        if (placement.thingDef != null)
    //        {
    //            sb.AppendLine($"---{placement.thingDef.LabelCap} {(placement.stuffDef != null ? $"(stuff:{placement.stuffDef.LabelCap})" : "")}");
    //        }
    //        if (placement.terrainDef != null)
    //        {
    //            sb.AppendLine($"----{placement.terrainDef.LabelCap}");
    //        }

    //        return sb.ToString().TrimEndNewlines();
    //    }
    //}

}