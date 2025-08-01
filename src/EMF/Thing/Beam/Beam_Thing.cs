using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Keyframe = UnityEngine.Keyframe;

namespace EMF
{
    public class BeamSegment : IExposable
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public Vector3 direction;

        public BeamSegment() { }

        public BeamSegment(Vector3 start, Vector3 end)
        {
            this.startPoint = start;
            this.endPoint = end;
            this.direction = (end - start).normalized;
        }

        public void ExposeData() 
        {
            Scribe_Values.Look(ref startPoint, "startPoint");
            Scribe_Values.Look(ref endPoint, "endPoint");
            Scribe_Values.Look(ref direction, "direction");
        }
    }

    public class BeamDef : ThingDef
    {
        public BeamParameters beamParameters;

        public BeamDef()
        {
            thingClass = typeof(Beam_Thing);
        }
    }

    public class Beam_Thing : ThingWithComps
    {
        public Pawn caster;
        public LocalTargetInfo target;
        public Ability sourceAbility;
       // protected IChannelableAbility Channelable => sourceAbility as IChannelableAbility;
        public BeamParameters parameters;

        protected int PreviousTick;
        public int CurrentIndex;
        public int LaunchTick;

        public float travelProgress;
        public Vector3 currentEndpoint;
        private Thing currentObstacle;
        private HashSet<Thing> hitThings = new HashSet<Thing>();
        public bool IsInBeamBattle = false;
        private HashSet<Thing> thingsInBeamPath = new HashSet<Thing>();

        private List<BeamSegment> beamSegments = new List<BeamSegment>();
        private BeamSegment activeSegment;

        private BeamCollisionTracker collisionTracker;
        private BeamRenderer renderer;
        public virtual GraphicData BeamMoteDef => this.parameters != null && this.parameters.beamGraphic != null ? this.parameters.beamGraphic : DefDatabase<ThingDef>.GetNamed("TestBeamMote").graphicData;

        public Vector3 CurrentDirection;

        #region Property
        public Vector3 Origin
        {
            get
            {
                Pawn pawn = this.caster;
                return (pawn != null) ? pawn.DrawPos : base.Position.ToVector3Shifted();
            }
        }

        public Vector3 InitialDestination
        {
            get
            {
                return this.target.HasThing ? this.target.Thing.DrawPos : this.target.Cell.ToVector3Shifted();
            }
        }

        public virtual int ExplosionRadius
        {
            get
            {
                BeamParameters beamParameters = this.parameters;
                return (beamParameters != null) ? beamParameters.explosionRadius : 2;
            }
        }

        public virtual DamageDef DamageType
        {
            get
            {
                if (this.parameters != null && this.parameters.damageType != null)
                {
                    return this.parameters.damageType;
                }
                return DamageDefOf.Burn;
            }
        }

        public virtual int BaseDamage
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters.baseDamage;
                }

                return 30;
            }
        }

        public virtual int DamagePerLevel
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters.damagePerLevel;
                }

                return 10;
            }
        }

        public virtual float BeamWidth
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters.beamWidth;
                }

                return 2;
            }
        }

        public virtual int TotalDamage => BaseDamage + DamagePerLevel;

        public virtual float TravelSpeed
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters.travelSpeed;
                }

                return 0.5f;
            }
        }

        public virtual bool PenetrateTargets
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters.penetrateTargets;
                }

                return true;
            }
        }

        public virtual int DamageTickInterval
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters.damageTickInterval;
                }

                return 30;
            }
        }

        #endregion

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.currentEndpoint = this.Origin;
                this.CurrentDirection = (this.InitialDestination - this.Origin).normalized;

                activeSegment = new BeamSegment(this.Origin, this.Origin);
                beamSegments.Add(activeSegment);
            }

            this.collisionTracker = new BeamCollisionTracker(this);
            this.renderer = new BeamRenderer(this);
        }

        public void SetParameters(BeamParameters beamParams)
        {
            this.parameters = beamParams;
        }

        public void UpdateEndpoint(Vector3 newEndpoint)
        {
            this.currentEndpoint = newEndpoint;
        }

        public void Deflect(Vector3 deflectionPoint, Vector3 newDirection)
        {
            activeSegment.endPoint = deflectionPoint;

            BeamSegment newSegment = new BeamSegment(deflectionPoint, deflectionPoint);
            beamSegments.Add(newSegment);
            activeSegment = newSegment;

            this.CurrentDirection = newDirection;
            this.travelProgress = 0;
        }

        protected override void Tick()
        {
            base.Tick();
            if (base.Spawned)
            {
                if (!this.IsInBeamBattle)
                {
                    if (this.ShouldDestroy())
                    {
                        CreateImpactExplosion(this.currentEndpoint.ToIntVec3());
                        this.Destroy(DestroyMode.Vanish);
                        return;
                    }
                    this.UpdateBeamPosition();

                    if (this.collisionTracker.CheckForCollisions())
                    {
                        this.CreateImpactExplosion(this.currentEndpoint.ToIntVec3());
                        this.Destroy(DestroyMode.Vanish);
                        return;
                    }
                }
                this.collisionTracker.UpdateThingsInBeamPath(this.thingsInBeamPath, this.beamSegments);
                this.DealDamageInBeamPath();
            }
        }

        private bool ShouldDestroy()
        {
            if (this.caster.DeadOrDowned)
            {
                return true;
            }

            float maxDistance = this.parameters?.maxTravelDistance ?? 30f;
            float totalDistance = 0f;
            foreach (BeamSegment segment in beamSegments)
            {
                totalDistance += Vector3.Distance(segment.startPoint, segment.endPoint);
            }

            if (totalDistance >= maxDistance)
            {
                return true;
            }

            //if (!this.Channelable.IsChanneling)
            //{
            //    return true;
            //}

            return false;
        }

        private void UpdateBeamPosition()
        {
            if (this.parameters.stopOnHit)
            {
                if (this.currentObstacle == null)
                {
                    if (collisionTracker.CheckForObstacleInPath(this.travelProgress - this.TravelSpeed, this.travelProgress, out Thing thing))
                    {
                        this.currentObstacle = thing;

                        if (CanDeflectBeam(thing))
                        {
                            Vector3 deflectionPoint = thing.DrawPos;
                            Vector3 newDirection = CalculateDeflectionDirection(thing);
                            Deflect(deflectionPoint, newDirection);
                            this.currentObstacle = null;
                            return;
                        }
                    }
                }
                else
                {
                    if (this.currentObstacle != null && this.currentObstacle.DestroyedOrNull())
                    {
                        this.currentObstacle = null;
                    }
                }
            }

            if (this.parameters != null && this.parameters.travelingBeam && this.currentObstacle == null)
            {
                this.travelProgress += this.TravelSpeed;
                if (activeSegment != null)
                {
                    activeSegment.endPoint = activeSegment.startPoint + this.CurrentDirection * this.travelProgress;
                    this.currentEndpoint = activeSegment.endPoint;
                }
            }
            else
            {
                if (this.parameters == null || !this.parameters.travelingBeam)
                {
                    this.currentEndpoint = this.InitialDestination;
                    if (activeSegment != null)
                    {
                        activeSegment.endPoint = this.currentEndpoint;
                    }
                }
            }
        }

        private void CreateImpactExplosion(IntVec3 impactPos)
        {
            if (this.parameters != null)
            {
                float damagePerLevel = 0f;
                if (this.sourceAbility != null)
                {
                    damagePerLevel = 1;
                }

                float damage = (float)this.parameters.impactBaseDamage + damagePerLevel;
                if (impactPos.InBounds(this.Map))
                {
                    GenExplosion.DoExplosion(impactPos, base.Map, this.parameters.impactExplosionRadius, this.DamageType, this.caster, Mathf.RoundToInt(damage), (float)this.parameters.impactArmourPen, null, null, null, null, null, 0f, 1, null, null, 255, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f, null, null, null, null);
                }
            }
        }

        public bool CanDamage(Thing thing)
        {
            if (thing == this || thing == this.caster || thing.Destroyed)
            {
                return false;
            }
            else
            {
                if (thing is Pawn pawn && pawn.Dead)
                {
                    return false;
                }
                return true;
            }
        }

        private void DealDamageInBeamPath()
        {
            bool flag = !this.IsHashIntervalTick(this.DamageTickInterval);
            if (!flag)
            {
                foreach (Thing thing in this.thingsInBeamPath)
                {
                    bool flag2 = (!this.PenetrateTargets && !this.hitThings.Add(thing)) || !this.CanDamage(thing);
                    if (!flag2)
                    {
                        float num = this.CalculateBeamDamage(thing);
                        DamageDef def = thing.def.mineable ? DamageDefOf.Mining : this.DamageType;

                        if (thing is Pawn pawn || thing is Building building)
                        {
                            EffecterDefOf.Deflect_Metal.Spawn(thing.Position, this.Map);
                        }

                        thing.TakeDamage(new DamageInfo(def, num / 60f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true, false));
                    }
                }
            }
        }

        private float CalculateBeamDamage(Thing thing)
        {
            float num = (float)(this.BaseDamage);
            BeamParameters beamParameters = this.parameters;
            bool flag = beamParameters != null && beamParameters.damageFalloff;
            if (flag)
            {
                float num2 = Vector3.Distance(this.Origin, thing.DrawPos);
                float num3 = Vector3.Distance(this.Origin, this.currentEndpoint);
                bool flag2 = num3 > 0f;
                if (flag2)
                {
                    float num4 = 1f - Mathf.Clamp01(num2 / num3) * 0.5f;
                    num *= num4;
                }
            }
            bool mineable = thing.def.mineable;
            if (mineable)
            {
                num *= 10f;
            }
            return num;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            foreach (BeamSegment segment in beamSegments)
            {
                float segmentDistance = Vector3.Distance(segment.startPoint, segment.endPoint);
                if (segmentDistance > 0.1f)
                {
                    this.renderer.DrawBeamSegment(segment, this.parameters);
                }
            }

            if (beamSegments.Count > 0)
            {
                this.renderer.DrawBeamStart(beamSegments[0].startPoint.SetToAltitude(AltitudeLayer.BuildingOnTop), this.parameters);
            }

            this.renderer.DrawBeamEnd(this.currentEndpoint.SetToAltitude(AltitudeLayer.BuildingOnTop), this.parameters);

            if (parameters.beamEndGraphic != null)
            {
                for (int i = 1; i < beamSegments.Count; i++)
                {
                    this.renderer.DrawBeamEnd(beamSegments[i].startPoint.SetToAltitude(AltitudeLayer.MoteLow), this.parameters);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.caster, "caster", false);
            Scribe_TargetInfo.Look(ref this.target, "target");
            Scribe_References.Look(ref this.sourceAbility, "sourceAbility", false);
            Scribe_References.Look(ref this.currentObstacle, "currentObstacle", false);
            Scribe_Values.Look(ref this.LaunchTick, "launchTick", 0, false);
            Scribe_Values.Look(ref this.CurrentIndex, "curInd", 0, false);
            Scribe_Values.Look(ref this.travelProgress, "travelProgress", 0f, false);
            Scribe_Values.Look(ref this.CurrentDirection, "directionVector", default(Vector3), false);
            Scribe_Values.Look(ref this.IsInBeamBattle, "isInBeamBattle", false, false);
            Scribe_Deep.Look(ref this.parameters, "parameters", Array.Empty<object>());
            Scribe_Collections.Look(ref this.hitThings, "hitThings", LookMode.Reference);
            Scribe_Collections.Look(ref beamSegments, "beamSegments", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars && beamSegments != null && beamSegments.Count > 0)
            {
                activeSegment = beamSegments[beamSegments.Count - 1];
            }
        }

        private bool CanDeflectBeam(Thing thing)
        {
            return false;
        }

        private Vector3 CalculateDeflectionDirection(Thing deflector)
        {
            return CurrentDirection.RotatedBy(Rand.Range(-90f, 90f));
        }
        public static Beam_Thing Create(Pawn caster, LocalTargetInfo target, Ability sourceAbility, BeamParameters parameters = null)
        {
            Beam_Thing kibeam_Thing = (Beam_Thing)ThingMaker.MakeThing(EMFDefOf.EMF_BeamThing, null);
            kibeam_Thing.caster = caster;
            kibeam_Thing.target = target;
            kibeam_Thing.sourceAbility = sourceAbility;
            kibeam_Thing.LaunchTick = Find.TickManager.TicksGame;
            kibeam_Thing.CurrentDirection = (target.CenterVector3 - caster.DrawPos).normalized;
            if (parameters != null)
            {
                kibeam_Thing.SetParameters(parameters);
            }
            
            return kibeam_Thing;
        }

        public static Beam_Thing Create(BeamDef beamThingDef, Pawn caster, LocalTargetInfo target, Ability sourceAbility)
        {
            Beam_Thing kibeam_Thing = (Beam_Thing)ThingMaker.MakeThing(beamThingDef, null);
            kibeam_Thing.caster = caster;
            kibeam_Thing.target = target;
            kibeam_Thing.sourceAbility = sourceAbility;
            kibeam_Thing.LaunchTick = Find.TickManager.TicksGame;
            kibeam_Thing.CurrentDirection = (target.CenterVector3 - caster.DrawPos).normalized;
            if (beamThingDef.beamParameters != null)
            {
                kibeam_Thing.SetParameters(beamThingDef.beamParameters);
            }

            return kibeam_Thing;
        }
    }

    public class BeamParameters : IExposable
    {
        public int explosionRadius = 2;
        public DamageDef damageType;
        public int baseDamage = 30;
        public int damagePerLevel = 10;
        public float travelSpeed = 0.5f;
        public float maxTravelDistance = 90f;
        public bool travelingBeam = true;
        public bool penetrateTargets = true;
        public int damageTickInterval = 60;
        public int beamWidth = 2;
        public bool damageFalloff = false;
        public Color beamColor = default(Color);
        public bool stopOnHit = true;
        public bool explodeOnImpact = false;
        public float impactExplosionRadius = 2f;
        public int impactBaseDamage = 50;
        public int impactArmourPen = 1;
        public int impactDamagePerLevel = 20;


        public bool tileBeam = true;
        public GraphicData beamGraphic;

        public GraphicData beamStartGraphic;
        public GraphicData beamEndGraphic;


        public EffecterDef beamStartEffecter;
        public EffecterDef beamEndEffecter;

        public float startMoteRotationOffset = 0f;
        public float endMoteRotationOffset = 0f;


        public AnimationCurve expandingBeam = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);

        public AnimationCurve focusedBeam = AnimationCurve.Linear(0f, 1f, 1f, 0.1f);

        public AnimationCurve pulsingBeam = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.25f, 0.5f),
            new Keyframe(0.5f, 1f),
            new Keyframe(0.75f, 0.5f),
            new Keyframe(1f, 1f)
        );

        public void ExposeData()
        {
            Scribe_Values.Look(ref explosionRadius, "explosionRadius", 2, false);
            Scribe_Defs.Look(ref damageType, "damageType");
            Scribe_Values.Look(ref baseDamage, "baseDamage", 30, false);
            Scribe_Values.Look(ref damagePerLevel, "damagePerLevel", 10, false);
            Scribe_Values.Look(ref travelSpeed, "travelSpeed", 0.5f, false);
            Scribe_Values.Look(ref maxTravelDistance, "maxTravelDistance", 30f, false);
            Scribe_Values.Look(ref travelingBeam, "travelingBeam", true, false);
            Scribe_Values.Look(ref penetrateTargets, "penetrateTargets", true, false);
            Scribe_Values.Look(ref damageTickInterval, "damageTickInterval", 60, false);
            Scribe_Values.Look(ref beamWidth, "beamWidth", 2, false);
            Scribe_Values.Look(ref damageFalloff, "damageFalloff", false, false);
            Scribe_Values.Look(ref beamColor, "beamColor", default(Color), false);
            Scribe_Values.Look(ref stopOnHit, "stopOnHit", true, false);
            Scribe_Values.Look(ref explodeOnImpact, "explodeOnImpact", false, false);
            Scribe_Values.Look(ref impactExplosionRadius, "impactExplosionRadius", 2f, false);
            Scribe_Values.Look(ref impactBaseDamage, "impactBaseDamage", 50, false);
            Scribe_Values.Look(ref impactArmourPen, "impactArmourPen", 10, false);
            Scribe_Values.Look(ref impactDamagePerLevel, "impactDamagePerLevel", 20, false);
            Scribe_Deep.Look(ref beamGraphic, "beamGraphic", Array.Empty<object>());
            Scribe_Values.Look(ref startMoteRotationOffset, "startMoteRotationOffset", 0f, false);
            Scribe_Values.Look(ref endMoteRotationOffset, "endMoteRotationOffset", 0f, false);
        }
    }
}