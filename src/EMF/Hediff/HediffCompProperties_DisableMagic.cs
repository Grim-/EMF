using Verse;

namespace EMF
{
    public class HediffCompProperties_DisableMagic : HediffCompProperties
    {
        public HediffCompProperties_DisableMagic()
        {
            compClass = typeof(HediffComp_DisableMagic);
        }
    }

    public class HediffComp_DisableMagic : HediffComp, IDisableMagic
    {
        public bool DisablesMagic => true;

        public override string CompLabelInBracketsExtra => "magic disabled";
    }




}