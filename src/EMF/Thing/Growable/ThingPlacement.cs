using Verse;

namespace EMF
{
    public class ThingPlacement : IExposable
    {
        public ThingDef thing;
        public IntVec2 position;
        public Rot4 rotation = Rot4.North;
        public ThingDef stuff;

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref thing, "thing");
            Scribe_Values.Look(ref position, "position");
            Scribe_Values.Look(ref rotation, "rotation");
            Scribe_Defs.Look(ref stuff, "stuff");
        }

    }
}