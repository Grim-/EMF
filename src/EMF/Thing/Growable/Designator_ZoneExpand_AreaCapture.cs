using RimWorld;
using Verse;

namespace EMF
{
    public class Designator_ZoneExpand_AreaCapture : Designator_ZoneAdd_AreaCapture
    {
        public Designator_ZoneExpand_AreaCapture()
        {
            this.defaultLabel = "DesignatorAreaCaptureExpand".Translate();
            this.defaultDesc = "DesignatorAreaCaptureExpandDesc".Translate();
            this.hotKey = KeyBindingDefOf.Misc8;
        }
    }
}