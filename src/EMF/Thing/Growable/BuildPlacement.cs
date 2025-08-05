using Verse;

namespace EMF
{
    public class BuildPlacement : IExposable
    {
        public ThingPlacement placement;
        public BuildingPartType partType;

        public void ExposeData()
        {
            Scribe_Deep.Look(ref placement, "placement");
            Scribe_Values.Look(ref partType, "partType");
        }
    }
}