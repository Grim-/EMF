using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class StructureLayoutDef : Def
    {
        public List<BuildingStage> stages = new List<BuildingStage>();
        public int LastStageIndex => stages.Count - 1;
        public IntVec2 MaxBuildSize => stages.Max(x => x.size);

        public BuildingStage GetStage(int index)
        {
            if (index < 0 || index > stages.Count)
            {
                return null;
            }

            return stages[index];
        }

        //Used by the editor, purely for data modelling, user is expected to export Def to xml. so dont bitch at me
        public StructureLayoutDef DeepCopy()
        {
            StructureLayoutDef copiedDef = new StructureLayoutDef();
            copiedDef.defName = this.defName;
            copiedDef.stages = new List<BuildingStage>();

            foreach (var stage in this.stages)
            {
                var newStage = new BuildingStage
                {
                    size = stage.size,
                    terrain = new List<TerrainPlacement>(
                        stage.terrain.Select(t => new TerrainPlacement
                        {
                            terrain = t.terrain,
                            position = t.position
                        })
                    ),
                    walls = new List<ThingPlacement>(
                        stage.walls.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    doors = new List<ThingPlacement>(
                        stage.doors.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    power = new List<ThingPlacement>(
                        stage.power.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    furniture = new List<ThingPlacement>(
                        stage.furniture.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    other = new List<ThingPlacement>(
                        stage.other.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    )
                };
                copiedDef.stages.Add(newStage);
            }
            return copiedDef;
        }

        public CellRect GetCellRect(IntVec3 position, int stageIndex = -1)
        {
            IntVec2 size;
            if (stageIndex == -1)
            {
                size = MaxBuildSize;
            }
            else
            {
                BuildingStage stage = GetStage(stageIndex);
                if (stage == null)
                {
                    return CellRect.Empty;
                }
                size = stage.size;
            }

            // Create a CellRect with the bottom-left corner at the specified position
            // and the width and height from the calculated size
            return new CellRect(
                position.x,
                position.z,
                size.x,
                size.z
            );
        }
    }
}