using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;

namespace EMF
{
    public class StructureBuilder
    {
        private static StructureBuilder instance;

        public static StructureBuilder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StructureBuilder();
                }
                return instance;
            }
        }

        private StructureLayoutDef structureDef;
        private int currentStage = 0;

        private StructureBuilder()
        {
            Reset();
        }

        public void SetStructure(StructureLayoutDef def)
        {
            this.structureDef = def;
        }

        public void Reset()
        {
            structureDef = new StructureLayoutDef();
            structureDef.defName = "CapturedStructure_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            structureDef.stages = new List<BuildingStage>();
            structureDef.stages.Add(new BuildingStage());
            currentStage = 0;
        }

        // Add a new stage
        public void AddStage()
        {
            structureDef.stages.Add(new BuildingStage());
            currentStage = structureDef.stages.Count - 1;
        }

        // Set current stage
        public void SetCurrentStage(int index)
        {
            if (index >= 0 && index < structureDef.stages.Count)
            {
                currentStage = index;
            }
        }

        public void CaptureAreaToCurrentStage(Map map, CellRect rect, IntVec3 origin, Func<Thing, bool> validator = null, Func<TerrainDef, bool> terrainValidator = null)
        {
            if (currentStage < 0 || currentStage >= structureDef.stages.Count)
            {
                currentStage = 0;
                if (structureDef.stages.Count == 0)
                {
                    structureDef.stages.Add(new BuildingStage());
                }
            }


            BuildingStage stage = structureDef.stages[currentStage];


            stage.size = new IntVec2(rect.Width, rect.Height);

            stage.terrain.Clear();
            stage.walls.Clear();
            stage.doors.Clear();
            stage.power.Clear();
            stage.furniture.Clear();
            stage.other.Clear();

            HashSet<int> processedThingIds = new HashSet<int>();

            foreach (IntVec3 cell in rect.Cells)
            {
                IntVec3 relPos = cell - origin;
                IntVec2 pos2D = new IntVec2(relPos.x, relPos.z);

                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null)
                {
                    if (terrainValidator != null && !terrainValidator.Invoke(terrain))
                    {
                        continue;
                    }

                    TerrainPlacement terrainPlacement = new TerrainPlacement
                    {
                        terrain = terrain,
                        position = pos2D
                    };
                    stage.terrain.Add(terrainPlacement);
                }

                // Process things in the cell
                List<Thing> things = cell.GetThingList(map);
                foreach (Thing thing in things)
                {
                    if (validator != null && !validator.Invoke(thing))
                    {
                        continue;
                    }

                    if (thing is Pawn || thing is Filth || thing is Mote || thing is Corpse || thing is Plant)
                        continue;

                    if (processedThingIds.Contains(thing.thingIDNumber))
                        continue;

                    if (cell != thing.Position)
                        continue;

                    processedThingIds.Add(thing.thingIDNumber);

                    ThingPlacement placement = new ThingPlacement
                    {
                        thing = thing.def,
                        position = pos2D,
                        rotation = thing.Rotation,
                        stuff = thing.Stuff
                    };

                    if (thing is Building_Door)
                    {
                        stage.doors.Add(placement);
                    }
                    else if (thing.def.fillPercent > 0.8f && thing.def.passability == Traversability.Impassable)
                    {
                        stage.walls.Add(placement);
                    }
                    else if (thing.def.building != null && thing.def.building.isPowerConduit)
                    {
                        stage.power.Add(placement);
                    }
                    else if (thing is Building && thing.def.building != null)
                    {
                        stage.furniture.Add(placement);
                    }
                    else
                    {
                        stage.other.Add(placement);
                    }
                }
            }
        }

        public StructureLayoutDef GetStructure()
        {
            return structureDef;
        }

        public int GetCurrentStageIndex()
        {
            return currentStage;
        }

        public BuildingStage GetCurrentStage()
        {
            return this.structureDef.stages[currentStage];
        }


        public string ToXmlString()
        {
            XmlDocument doc = new XmlDocument();

            XmlElement rootElement = doc.CreateElement("MagicAndMyths.StructureLayoutDef");
            doc.AppendChild(rootElement);

            XmlElement defNameElement = doc.CreateElement("defName");
            defNameElement.InnerText = structureDef.defName;
            rootElement.AppendChild(defNameElement);

            XmlElement stagesElement = doc.CreateElement("stages");
            rootElement.AppendChild(stagesElement);

            foreach (BuildingStage stage in structureDef.stages)
            {
                XmlElement stageElement = doc.CreateElement("li");
                stagesElement.AppendChild(stageElement);

                XmlElement sizeElement = doc.CreateElement("size");
                stageElement.AppendChild(sizeElement);

                XmlElement sizeXElement = doc.CreateElement("x");
                sizeXElement.InnerText = stage.size.x.ToString();
                sizeElement.AppendChild(sizeXElement);

                XmlElement sizeZElement = doc.CreateElement("z");
                sizeZElement.InnerText = stage.size.z.ToString();
                sizeElement.AppendChild(sizeZElement);

                XmlElement terrainElement = doc.CreateElement("terrain");
                stageElement.AppendChild(terrainElement);

                foreach (TerrainPlacement terrain in stage.terrain)
                {
                    XmlElement terrainItemElement = doc.CreateElement("li");
                    terrainElement.AppendChild(terrainItemElement);

                    XmlElement terrainDefElement = doc.CreateElement("terrain");
                    terrainDefElement.InnerText = terrain.terrain.defName;
                    terrainItemElement.AppendChild(terrainDefElement);

                    XmlElement positionElement = doc.CreateElement("position");
                    terrainItemElement.AppendChild(positionElement);

                    XmlElement posXElement = doc.CreateElement("x");
                    posXElement.InnerText = terrain.position.x.ToString();
                    positionElement.AppendChild(posXElement);

                    XmlElement posZElement = doc.CreateElement("z");
                    posZElement.InnerText = terrain.position.z.ToString();
                    positionElement.AppendChild(posZElement);
                }

                AddThingCategory(doc, stageElement, "walls", stage.walls);

                AddThingCategory(doc, stageElement, "doors", stage.doors);

                AddThingCategory(doc, stageElement, "power", stage.power);

                AddThingCategory(doc, stageElement, "furniture", stage.furniture);

                AddThingCategory(doc, stageElement, "other", stage.other);
            }

            return FormatXml(doc);
        }

        public static string FormatXml(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace
            };

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }

            return sb.ToString();
        }

        private void AddThingCategory(XmlDocument doc, XmlElement parent, string categoryName, List<ThingPlacement> items)
        {
            XmlElement categoryElement = doc.CreateElement(categoryName);
            parent.AppendChild(categoryElement);

            foreach (ThingPlacement item in items)
            {
                XmlElement itemElement = doc.CreateElement("li");
                categoryElement.AppendChild(itemElement);


                XmlElement thingElement = doc.CreateElement("thing");
                thingElement.InnerText = item.thing.defName;
                itemElement.AppendChild(thingElement);

                XmlElement posElement = doc.CreateElement("position");
                itemElement.AppendChild(posElement);

                XmlElement posXElement = doc.CreateElement("x");
                posXElement.InnerText = item.position.x.ToString();
                posElement.AppendChild(posXElement);

                XmlElement posZElement = doc.CreateElement("z");
                posZElement.InnerText = item.position.z.ToString();
                posElement.AppendChild(posZElement);

                if (item.rotation != Rot4.Invalid)
                {
                    XmlElement rotationElement = doc.CreateElement("rotation");
                    string rotationName = "";
                    switch (item.rotation.AsInt)
                    {
                        case 0: rotationName = "North"; break;
                        case 1: rotationName = "East"; break;
                        case 2: rotationName = "South"; break;
                        case 3: rotationName = "West"; break;
                    }
                    rotationElement.InnerText = rotationName;
                    itemElement.AppendChild(rotationElement);
                }

                // Add stuff if present
                if (item.stuff != null)
                {
                    XmlElement stuffElement = doc.CreateElement("stuff");
                    stuffElement.InnerText = item.stuff.defName;
                    itemElement.AppendChild(stuffElement);
                }
            }
        }


        public static bool BuildStructure(StructureLayoutDef structureDef, IntVec3 origin, int stageIndex, Map map, Faction faction = null)
        {
            if (structureDef == null || map == null)
            {
                Log.Error("BuildStructure called with null structure definition or map");
                return false;
            }

            BuildingStage stage = structureDef.GetStage(stageIndex);
            if (stage == null)
            {
                Log.Error($"Invalid stage index {stageIndex} for structure {structureDef.defName}");
                return false;
            }

            CellRect structureBounds = new CellRect(
                origin.x,
                origin.z,
                stage.size.x,
                stage.size.z);

            if (!structureBounds.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
            {
                Log.Error($"Structure {structureDef.defName} does not fit on the map at position {origin}");
                return false;
            }

            if (stage.destroyPreviousStage && stageIndex > 0)
            {
                BuildingStage prevStage = structureDef.GetStage(stageIndex - 1);
                if (prevStage != null)
                {
                    foreach (IntVec3 cell in structureBounds.Cells)
                    {
                        foreach (Thing thing in cell.GetThingList(map).ToList())
                        {
                            if (thing is Building)
                            {
                                thing.Destroy(DestroyMode.Vanish);
                            }
                        }
                    }
                }
            }

            foreach (TerrainPlacement terrainPlacement in stage.terrain)
            {
                IntVec3 position = new IntVec3(
                    origin.x + terrainPlacement.position.x,
                    origin.y,
                    origin.z + terrainPlacement.position.z);

                if (position.InBounds(map))
                {
                    map.terrainGrid.SetTerrain(position, terrainPlacement.terrain);
                }
            }

            PlaceThings(stage.walls, origin, map, faction);
            PlaceThings(stage.doors, origin, map, faction);
            PlaceThings(stage.power, origin, map, faction);
            PlaceThings(stage.furniture, origin, map, faction);
            PlaceThings(stage.other, origin, map, faction);

            return true;
        }

        private static void PlaceThings(List<ThingPlacement> placements, IntVec3 origin, Map map, Faction faction)
        {
            foreach (ThingPlacement placement in placements)
            {
                if (placement.thing == null)
                    continue;

                IntVec3 position = new IntVec3(
                    origin.x + placement.position.x,
                    origin.y,
                    origin.z + placement.position.z);

                if (position.InBounds(map))
                {
                    Thing thing = ThingMaker.MakeThing(placement.thing, placement.stuff);
                    if (thing.Faction != null && faction != null)
                    {
                        thing.SetFaction(faction);
                    }
                    GenSpawn.Spawn(thing, position, map, placement.rotation);
                }
            }
        }
    }
}