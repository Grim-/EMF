using RimWorld;
using UnityEngine;
using Verse;

namespace EMF
{
    [StaticConstructorOnStartup]
    public class Meteor : Thing
    {
        // Textures
        private static readonly Texture2D meteorTexture = ContentFinder<Texture2D>.Get("Misc/Meteor");
        private static readonly Texture2D shadowTexture = ContentFinder<Texture2D>.Get("Misc/MeteorShadow");


        protected GraphicData MeteorGraphicData;
        protected GraphicData MeteorShadowGraphicData;

        // Animation properties
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Vector3 currentPosition;
        private int currentTick = 0;
        private int totalDurationTicks = 1000;
        private bool hasImpacted = false;
        private float scale = 1.5f;
        private float startingHeight = 200f;

        // Explosion properties
        private int explosionRadius = 25;
        private int damageAmount = 150;
        private DamageDef damageDef = DamageDefOf.Bomb;
        protected bool Launched = false;

        private EffecterDef impactEffecter = null;

        private Vector2 startSize = new Vector2(5, 5);
        private Vector2 finalSize = new Vector2(0.5f, 0.5f);

        public override Vector2 DrawSize
        {
            get
            {
                float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);
                return Vector2.Lerp(finalSize, startSize, progress);
            }
        }

        private float ShadowSize
        {
            get
            {
                float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);
                return DrawSize.x * 1.4f;
            }
        }

        public void Launch(IntVec3 target, int ticksToImpact = 1000)
        {
            targetPosition = target.ToVector3Shifted();
            startPosition = new Vector3(targetPosition.x, startingHeight, targetPosition.z - startingHeight);
            currentPosition = startPosition;
            currentTick = 0;
            this.totalDurationTicks = ticksToImpact;
            Launched = true;
        }

        public void SetImpactEffecter(EffecterDef effecterDef)
        {
            impactEffecter = effecterDef;
        }

        public void SetSize(Vector2 startSize, Vector2 endSize)
        {
            this.startSize = startSize;
            this.finalSize = endSize;
        }

        public void SetRadius(int radius)
        {
            this.explosionRadius = radius;
        }

        public void SetDamageDef(DamageDef damageDef)
        {
            this.damageDef = damageDef;
        }

        public void SetDamage(int damage)
        {
            this.damageAmount = damage;
        }

        protected override void Tick()
        {
            if (hasImpacted || !Launched)
                return;

            currentTick++;

            float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);

            currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            currentPosition.y = Mathf.Lerp(startPosition.y, 0f, progress);

            // Check for impact
            if (currentTick >= totalDurationTicks && !hasImpacted)
            {
                Impact();
            }
        }

        private void Impact()
        {
            if (impactEffecter != null)
            {
                impactEffecter.Spawn(Position, Map);
            }

            hasImpacted = true;

            TargetUtil.ApplyDamageInRadius(damageDef, damageAmount, 0, Position, Map, explosionRadius, this.Faction, FriendlyFireSettings.All());
            this.DeSpawn();
        }


        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 shadowPos = new Vector3(targetPosition.x, 0, targetPosition.z);
            float shadowSize = ShadowSize;

            Graphics.DrawMesh(
                MeshPool.plane10,
                Matrix4x4.TRS(
                    shadowPos,
                    Quaternion.identity,
                    new Vector3(shadowSize, 1f, shadowSize)),
                FadedMaterialPool.FadedVersionOf(MeteorShadowGraphicData.Graphic.MatSingle, 1f),
                0);

            float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);
            if (progress > 0.7f)
            {
                Vector2 size = DrawSize;
                Graphics.DrawMesh(
                    MeshPool.plane10,
                    Matrix4x4.TRS(
                        currentPosition,
                        Quaternion.identity,
                        new Vector3(size.x * scale, 1f, size.y * scale)),
                    FadedMaterialPool.FadedVersionOf(MeteorGraphicData.Graphic.MatSingle, 1f),
                    0);
            }
        }


        //public static Meteor Launch(IntVec3 position, Map map, Vector2 startSize, Vector2 endSize, int radius = 5, int ticksToImpact = 1000, DamageDef customDamageDef = null, int customDamage = -1, EffecterDef impactEffectDef = null)
        //{
        //    Meteor meteor = (Meteor)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMyths_Meteor);
        //    meteor.SetSize(startSize, endSize);


        //    if (customDamageDef != null)
        //    {
        //        meteor.SetDamageDef(customDamageDef);
        //    }

        //    meteor.SetRadius(radius);

        //    if (customDamage > 0)
        //    {
        //        meteor.SetDamage(customDamage);
        //    }


        //    if (impactEffectDef != null)
        //    {
        //        meteor.SetImpactEffecter(impactEffectDef);
        //    }


        //    GenSpawn.Spawn(meteor, position, map);
        //    meteor.Launch(position, ticksToImpact);
        //    return meteor;
        //}

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref startPosition, "startPosition");
            Scribe_Values.Look(ref targetPosition, "targetPosition");
            Scribe_Values.Look(ref currentPosition, "currentPosition");
            Scribe_Values.Look(ref currentTick, "currentTick");
            Scribe_Values.Look(ref totalDurationTicks, "totalDurationTicks");
            Scribe_Values.Look(ref hasImpacted, "hasImpacted");
            Scribe_Values.Look(ref Launched, "isLaunched");
            Scribe_Defs.Look(ref impactEffecter, "impactEffecter");
        }
    }
}
