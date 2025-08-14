using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ThingFlyer : Thing, IThingHolder
    {
        protected ThingOwner<Thing> innerContainer;
        protected Vector3 startVec;

        protected TargetInfo target;
        protected IntVec3 destCell;
        protected float flightDistance;
        protected int ticksFlightTime = 120;
        protected int ticksFlying;
        protected EffecterDef flightEffecterDef;
        protected SoundDef soundLanding;
        protected Effecter flightEffecter;
        protected int positionLastComputedTick = -1;
        protected Vector3 groundPos;
        protected Vector3 effectivePos;
        protected float effectiveHeight;

        protected Pawn throwingPawn = null;
        public Action<IntVec3, Pawn> OnBeforeRespawn;
        public Action<IntVec3, Thing, Pawn> OnRespawn;
        public Action<int, IntVec3, Map, Thing> OnFlightTick;

        protected Thing FlyingThing
        {
            get
            {
                if (this.innerContainer.InnerListForReading.Count <= 0)
                {
                    return null;
                }
                return this.innerContainer.InnerListForReading[0];
            }
        }

        public Graphic FlyingThingGraphic = null;

        public Vector3 DestinationPos
        {
            get
            {
                return this.destCell.ToVector3Shifted();
            }
        }
        public override Vector3 DrawPos
        {
            get
            {
                this.RecomputePosition();
                return this.effectivePos;
            }
        }

        protected bool ThingWasSelected = false;
        protected bool PawnWasDrafted = false;

        public ThingFlyer()
        {
            this.innerContainer = new ThingOwner<Thing>(this);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                float num = Mathf.Max(this.flightDistance, 1f) / this.def.pawnFlyer.flightSpeed;
                num = Mathf.Max(num, this.def.pawnFlyer.flightDurationMin);
                this.ticksFlightTime = num.SecondsToTicks();
                this.ticksFlying = 0;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Effecter effecter = this.flightEffecter;
            if (effecter != null)
            {
                effecter.Cleanup();
            }
            base.Destroy(mode);
        }

        protected void RecomputePosition()
        {
            if (this.positionLastComputedTick == this.ticksFlying)
            {
                return;
            }
            this.positionLastComputedTick = this.ticksFlying;
            float t = (float)this.ticksFlying / (float)this.ticksFlightTime;
            float t2 = this.def.pawnFlyer.Worker.AdjustedProgress(t);
            this.effectiveHeight = this.def.pawnFlyer.Worker.GetHeight(t2);
            this.groundPos = Vector3.Lerp(this.startVec, this.DestinationPos, t2);
            Vector3 b = Altitudes.AltIncVect * this.effectiveHeight;
            Vector3 b2 = Vector3.forward * (this.def.pawnFlyer.heightFactor * this.effectiveHeight);
            this.effectivePos = this.groundPos + b + b2;
            base.Position = this.groundPos.ToIntVec3();
        }

        public void SetThrowingPawn(Pawn pawn)
        {
            this.throwingPawn = pawn;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        protected virtual void RespawnThing()
        {
            Thing flyingThing = this.FlyingThing;
            if (flyingThing == null)
            {
                return;
            }

            Map map = this.Map;
            if (map == null)
            {
                return;
            }

            OnBeforeRespawn?.Invoke(this.destCell, this.throwingPawn);
            this.innerContainer.TryDrop(flyingThing, this.destCell, map, ThingPlaceMode.Near, out Thing thing, null, null, false);
            OnRespawn?.Invoke(this.destCell, flyingThing, this.throwingPawn);

            if (ThingWasSelected)
            {
                Find.Selector.Select(thing);
            }

            if (PawnWasDrafted && thing is Pawn pawn)
            {
                pawn.drafter.Drafted = PawnWasDrafted;
            }
        }

        protected override void Tick()
        {
            if (this.flightEffecter == null && this.flightEffecterDef != null)
            {
                this.flightEffecter = this.flightEffecterDef.Spawn();
                this.flightEffecter.Trigger(this, TargetInfo.Invalid, -1);
            }
            else
            {
                Effecter effecter = this.flightEffecter;
                if (effecter != null)
                {
                    effecter.EffectTick(this, TargetInfo.Invalid);
                }
            }

            if (this.FlyingThing != null)
            {
                OnFlightTick?.Invoke(this.ticksFlying, this.DrawPos.ToIntVec3(), Map, FlyingThing);
            }

            if (this.ticksFlying >= this.ticksFlightTime)
            {
                this.RespawnThing();
                this.Destroy(DestroyMode.KillFinalize);
            }
            else
            {
                if (this.ticksFlying % 5 == 0)
                {
                    this.CheckDestination();
                }
                this.innerContainer.DoTick();
            }
            this.ticksFlying++;
        }

        private void CheckDestination()
        {
            if (!JumpUtility.ValidJumpTarget(this, base.Map, this.destCell))
            {
                int num = GenRadial.NumCellsInRadius(3.9f);
                for (int i = 0; i < num; i++)
                {
                    IntVec3 cell = this.destCell + GenRadial.RadialPattern[i];
                    if (JumpUtility.ValidJumpTarget(this, base.Map, cell))
                    {
                        this.destCell = cell;
                        return;
                    }
                }
            }
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            this.RecomputePosition();

            if (this.FlyingThingGraphic != null)
            {
                this.FlyingThingGraphic.Draw(this.effectivePos, this.Rotation, this, 0f);
            }
            else
            {
                Thing flyingThing = this.FlyingThing;
                if (flyingThing != null)
                {
                    flyingThing.DynamicDrawPhaseAt(phase, this.effectivePos, false);
                }
            }

            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            this.DrawShadow(this.groundPos, this.effectiveHeight);
        }

        private void DrawShadow(Vector3 drawLoc, float height)
        {
            Material shadowMaterial = this.def.pawnFlyer.ShadowMaterial;
            if (shadowMaterial == null)
            {
                return;
            }
            float num = Mathf.Lerp(1f, 0.6f, height);
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawLoc, Quaternion.identity, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
        }

        /// <summary>
        /// Creates and set up new a ThingFlyer, does not spawn nor start the flyer, see ThingFlyer.LaunchFlyer
        /// </summary>
        /// <param name="flyingDef"></param>
        /// <param name="thing"></param>
        /// <param name="destCell"></param>
        /// <param name="map"></param>
        /// <param name="flightEffecterDef"></param>
        /// <param name="landingSound"></param>
        /// <param name="throwerPawn"></param>
        /// <param name="overrideStartVec"></param>
        /// <param name="graphicOverride">Optional graphic to display instead of the flying thing</param>
        /// <returns></returns>
        public static ThingFlyer MakeFlyer(ThingDef flyingDef, Thing thing, IntVec3 destCell, Map map, EffecterDef flightEffecterDef, SoundDef landingSound, Pawn throwerPawn, Vector3? overrideStartVec = null, bool defaultThrowBehaviour = true, Graphic graphicOverride = null)
        {
            ThingFlyer thingFlyer = (ThingFlyer)ThingMaker.MakeThing(flyingDef, null);
            Vector3 startVec = overrideStartVec ?? (throwerPawn?.TrueCenter() ?? thing.TrueCenter());
            thingFlyer.startVec = startVec;
            thingFlyer.flightDistance = startVec.ToIntVec3().DistanceTo(destCell);
            thingFlyer.Rotation = thing.Rotation;
            thingFlyer.destCell = destCell;
            thingFlyer.flightEffecterDef = flightEffecterDef;
            thingFlyer.soundLanding = landingSound;
            thingFlyer.SetThrowingPawn(throwerPawn);
            thingFlyer.FlyingThingGraphic = graphicOverride;
            return thingFlyer;
        }

        /// <summary>
        /// Creates and set up new a ThingFlyer, does not spawn nor start the flyer, see ThingFlyer.LaunchFlyer
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="destCell"></param>
        /// <param name="map"></param>
        /// <param name="flightEffecterDef"></param>
        /// <param name="landingSound"></param>
        /// <param name="throwerPawn"></param>
        /// <param name="overrideStartVec"></param>
        /// <param name="graphicOverride">Optional graphic to display instead of the flying thing</param>
        /// <returns></returns>
        public static ThingFlyer MakeFlyer(Thing thing, IntVec3 destCell, Map map, EffecterDef flightEffecterDef, SoundDef landingSound, Pawn throwerPawn, Vector3? overrideStartVec = null, bool defaultThrowBehaviour = true, Graphic graphicOverride = null)
        {
            return MakeFlyer(EMFDefOf.EMF_ThingFlyer, thing, destCell, map, flightEffecterDef, landingSound, throwerPawn, overrideStartVec, defaultThrowBehaviour, graphicOverride);
        }

        /// <summary>
        /// Actually triggers the flyer to move, despawns the thing if not already
        /// </summary>
        /// <param name="Flyer"></param>
        /// <param name="thing"></param>
        /// <param name="spawnCell"></param>
        /// <param name="destCell"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static ThingFlyer LaunchFlyer(ThingFlyer Flyer, Thing thing, IntVec3 spawnCell, Map map)
        {
            bool wasSelected = Find.Selector.IsSelected(thing);
            bool wasDrafted = thing is Pawn pawn && pawn.drafter != null ? pawn.drafter.Drafted : false;
            if (thing.Spawned)
            {
                thing.DeSpawn(DestroyMode.Vanish);
            }

            if (!Flyer.innerContainer.TryAdd(thing, true))
            {
            }

            Flyer.PawnWasDrafted = wasDrafted;
            Flyer.ThingWasSelected = wasSelected;

            GenSpawn.Spawn(Flyer, spawnCell, map);
            return Flyer;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref this.startVec, "startVec", default(Vector3), false);
            Scribe_Values.Look<IntVec3>(ref this.destCell, "destCell", default(IntVec3), false);
            Scribe_Values.Look<float>(ref this.flightDistance, "flightDistance", 0f, false);
            Scribe_Values.Look<int>(ref this.ticksFlightTime, "ticksFlightTime", 0, false);
            Scribe_Values.Look<int>(ref this.ticksFlying, "ticksFlying", 0, false);
            Scribe_References.Look(ref throwingPawn, "throwingPawn");
            Scribe_Defs.Look<EffecterDef>(ref this.flightEffecterDef, "flightEffecterDef");
            Scribe_Defs.Look<SoundDef>(ref this.soundLanding, "soundLanding");
            Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }
    }
}