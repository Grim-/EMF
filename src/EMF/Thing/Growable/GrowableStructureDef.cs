using UnityEngine;
using Verse;

namespace EMF
{
    public class GrowableStructureDef : ThingDef
    {
        public StructureLayoutDef structureLayout;
        public ThingDef rootDef;
        public Color previewColor;
        public int growthDays = 3;

        public int ticksBetweenPlacements = 10;

        //overrides stuff materials 
        public ThingDef defaultWallStuff;
        public ThingDef defaultDoorStuff;
        public ThingDef defaultFurnitureStuff;
        public TerrainDef defaultFloorStuff;
        public GrowableStructureDef()
        {
            thingClass = typeof(Building_GrowableStructure);
        }
    }
}