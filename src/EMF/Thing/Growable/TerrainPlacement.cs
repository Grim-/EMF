using Verse;

namespace EMF
{
    public class TerrainPlacement : ThingPlacement
    {
        public TerrainDef terrain;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref terrain, "terrain");
        }
    }
}