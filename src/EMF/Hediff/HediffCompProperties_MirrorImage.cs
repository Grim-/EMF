using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class Thing_MirrorImage : Thing
    {
        private Pawn sourcePawn;
        private float opacity = 0.5f;
        private int ticksToLive = -1;
        private int imageCount = 6;
        private List<Vector3> imagePositions = new List<Vector3>();
        private float angleOffset = 0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref sourcePawn, "sourcePawn");
            Scribe_Values.Look(ref opacity, "opacity", 0.5f);
            Scribe_Values.Look(ref ticksToLive, "ticksToLive", -1);
        }

        public void Setup(Pawn pawn, float imageOpacity, int lifetimeTicks = -1)
        {
            sourcePawn = pawn;
            opacity = imageOpacity;
            ticksToLive = lifetimeTicks;
            UpdateImagePositions();
        }

        protected override void Tick()
        {
            base.Tick();
            UpdateImagePositions();
            if (ticksToLive > 0)
            {
                ticksToLive--;
                if (ticksToLive <= 0)
                {
                    Destroy();
                }
            }

            if (sourcePawn == null || sourcePawn.Destroyed || sourcePawn.Dead)
            {
                Destroy();
            }
        }
        private void UpdateImagePositions()
        {
            imagePositions.Clear();
            float angleStep = 360f / imageCount;
            for (int i = 0; i < imageCount; i++)
            {
                float angle = (angleStep * i + angleOffset) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * 4,
                    0f,
                    Mathf.Sin(angle) * 4
                );
                imagePositions.Add(offset);
            }
        }
        public void DrawAt(Vector3 drawLoc)
        {
            foreach (var offset in imagePositions)
            {
                Vector3 mirrorPos = drawLoc + offset;
                DrawPawnAt(mirrorPos, 1);
            }
        }

        private void DrawPawnAt(Vector3 drawLoc, float opacity)
        {
            var pawn = sourcePawn;
            var bodyGrpahic = pawn.Drawer.renderer.BodyGraphic;
            var headGraphic = pawn.Drawer.renderer.HeadGraphic;
            bodyGrpahic.Draw(drawLoc, pawn.Rotation, pawn);
            headGraphic.Draw(drawLoc + pawn.Drawer.renderer.BaseHeadOffsetAt(pawn.Rotation), pawn.Rotation, pawn);
        }
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            DrawAt(drawLoc);
        }
    }
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