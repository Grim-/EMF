using RimWorld;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CarryingFlyer : ThingFlyer
    {
        private bool carriedThingWasSelected;
        private bool carriedThingWasDrafted;

        protected Thing CarriedThing
        {
            get
            {
                if (this.innerContainer.InnerListForReading.Count <= 1)
                {
                    return null;
                }
                return this.innerContainer.InnerListForReading[1];
            }
        }

        protected override void RespawnThing()
        {
            Thing flyingThing = this.FlyingThing;
            Thing carriedThing = this.CarriedThing;

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

            this.innerContainer.TryDrop(flyingThing, this.destCell, map, ThingPlaceMode.Near, out Thing mainThing, null, null, false);

            if (carriedThing != null)
            {
                this.innerContainer.TryDrop(carriedThing, this.destCell, map, ThingPlaceMode.Near, out Thing droppedCarried, null, null, false);

                if (carriedThingWasSelected)
                {
                    Find.Selector.Select(droppedCarried);
                }

                if (carriedThingWasDrafted && droppedCarried is Pawn carriedPawn)
                {
                    carriedPawn.drafter.Drafted = carriedThingWasDrafted;
                }
            }

            OnRespawn?.Invoke(this.destCell, flyingThing, this.throwingPawn);

            if (ThingWasSelected)
            {
                Find.Selector.Select(mainThing);
            }

            if (PawnWasDrafted && mainThing is Pawn pawn)
            {
                pawn.drafter.Drafted = PawnWasDrafted;
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
                Thing carriedThing = this.CarriedThing;

                if (flyingThing != null)
                {
                    flyingThing.DynamicDrawPhaseAt(phase, this.effectivePos, false);
                }

                if (carriedThing != null)
                {
                    Vector3 carriedOffset = new Vector3(0.3f, 0f, -0.3f);
                    carriedThing.DynamicDrawPhaseAt(phase, this.effectivePos + carriedOffset, false);
                }
            }

            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
        }

        public static CarryingFlyer MakeCarryingFlyer(ThingDef flyingDef, Thing carrier, Thing carried, IntVec3 destCell, Map map, EffecterDef flightEffecterDef, SoundDef landingSound, Pawn throwerPawn, Vector3? overrideStartVec = null, Graphic graphicOverride = null)
        {
            CarryingFlyer flyer = (CarryingFlyer)ThingMaker.MakeThing(flyingDef, null);
            Vector3 startVec = overrideStartVec ?? (throwerPawn?.TrueCenter() ?? carrier.TrueCenter());
            flyer.startVec = startVec;
            flyer.flightDistance = startVec.ToIntVec3().DistanceTo(destCell);
            flyer.Rotation = carrier.Rotation;
            flyer.destCell = destCell;
            flyer.flightEffecterDef = flightEffecterDef;
            flyer.soundLanding = landingSound;
            flyer.SetThrowingPawn(throwerPawn);
            flyer.FlyingThingGraphic = graphicOverride;

            flyer.carriedThingWasSelected = Find.Selector.IsSelected(carried);
            flyer.carriedThingWasDrafted = carried is Pawn p && p.drafter != null ? p.drafter.Drafted : false;

            return flyer;
        }

        public static CarryingFlyer LaunchCarryingFlyer(CarryingFlyer flyer, Thing carrier, Thing carried, IntVec3 spawnCell, Map map)
        {
            bool carrierWasSelected = Find.Selector.IsSelected(carrier);
            bool carrierWasDrafted = carrier is Pawn pawn && pawn.drafter != null ? pawn.drafter.Drafted : false;

            if (carrier.Spawned)
            {
                carrier.DeSpawn(DestroyMode.Vanish);
            }

            if (carried.Spawned)
            {
                carried.DeSpawn(DestroyMode.Vanish);
            }

            if (!flyer.innerContainer.TryAdd(carrier, true))
            {
            }

            if (!flyer.innerContainer.TryAdd(carried, true))
            {
            }

            flyer.PawnWasDrafted = carrierWasDrafted;
            flyer.ThingWasSelected = carrierWasSelected;

            GenSpawn.Spawn(flyer, spawnCell, map);
            return flyer;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref carriedThingWasSelected, "carriedThingWasSelected");
            Scribe_Values.Look(ref carriedThingWasDrafted, "carriedThingWasDrafted");
        }
    }
}