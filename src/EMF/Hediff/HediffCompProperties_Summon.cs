using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_Summon : HediffCompProperties
    {
        public float followDistance = 20f;

        public HediffCompProperties_Summon()
        {
            compClass = typeof(HediffComp_Summon);
        }
    }

    public class HediffComp_Summon : HediffComp
    {
        public HediffCompProperties_Summon Props => (HediffCompProperties_Summon)props;

        protected Pawn _Summoner = null;
        public Pawn Summoner { get => _Summoner; set => _Summoner = value; }
        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            return base.CompGetGizmos();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref _Summoner, "summoner");
        }
    }
}