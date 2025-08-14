using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class HediffCompProperties_MirrorImage : HediffCompProperties
    {
        public IntRange imageAmount = new IntRange(4, 6);
        public float imageRadius = 6f;
        public float imageOpacity = 0.5f;

        public HediffCompProperties_MirrorImage()
        {
            compClass = typeof(HediffComp_MirrorImage);
        }
    }

    public class HediffComp_MirrorImage : HediffComp
    {
        public HediffCompProperties_MirrorImage Props => (HediffCompProperties_MirrorImage)props;

        private int imageCount = 6;
        private List<Vector3> imagePositions = new List<Vector3>();
        private float angleOffset = 0f;

        protected Thing_MirrorImage MirrorImage;

        public override void CompPostMake()
        {
            base.CompPostMake();
            MirrorImage = (Thing_MirrorImage)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("EMF_MirrorImageDrawer"));
            MirrorImage.Setup(this.parent.pawn, 1, -1);

            GenSpawn.Spawn(MirrorImage, this.parent.pawn.Position, this.parent.pawn.Map);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref imageCount, "imageCount", 0);
            Scribe_Values.Look(ref angleOffset, "angleOffset", 0f);
        }
    }
}