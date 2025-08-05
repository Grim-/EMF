using Verse;

namespace EMF
{
    public class DropItemChance
    {
        public ThingDef thingDef;
        public IntRange count = new IntRange(1, 1);
        public FloatRange chance = new FloatRange(1f, 1f);
    }
}
