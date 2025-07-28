using Verse;

namespace EMF
{
    public class HediffCompProperties_DisableRessurection : HediffCompProperties
    {
        public HediffCompProperties_DisableRessurection()
        {
            compClass = typeof(HediffComp_DisableRessurection);
        }
    }

    public class HediffComp_DisableRessurection : HediffComp, IDisableRessurection
    {
        public bool DisablesRessurection => true;

        public override string CompLabelInBracketsExtra => "resurrection blocked";
    }
}