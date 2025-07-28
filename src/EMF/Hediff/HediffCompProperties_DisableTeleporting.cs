using Verse;

namespace EMF
{
    public class HediffCompProperties_DisableTeleporting : HediffCompProperties
    {
        public HediffCompProperties_DisableTeleporting()
        {
            compClass = typeof(HediffComp_DisableTeleporting);
        }
    }

    public class HediffComp_DisableTeleporting : HediffComp, IDisableTeleportingAbilities
    {
        public bool DisablesTeleporting => true;

        public override string CompLabelInBracketsExtra => "teleporting disabled";
    }

}