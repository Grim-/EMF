using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class FancyTeleporter : Thing, IThingHolder
    {
        private ThingOwner<Thing> innerContainer;
        public Pawn teleportingPawn;
        public IntVec3 originPosition;
        public IntVec3 destinationPosition;
        public Map originMap;
        public Map destinationMap;

        public int delayTicks = 60;

        public EffecterDef originEffecter;
        public EffecterDef destinationEffecter;

        private int spawnTick = -1;
        private bool hasDespawned = false;
        private bool hasRespawned = false;



        private bool wasSelected = false;
        private bool wasDrafted = false;

        public FancyTeleporter()
        {
            this.innerContainer = new ThingOwner<Thing>(this);
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                spawnTick = Find.TickManager.TicksGame;
                Position = originPosition;

                if (originEffecter != null)
                {
                    Effecter effect = originEffecter.Spawn();
                    effect.Trigger(new TargetInfo(originPosition, originMap), new TargetInfo(originPosition, originMap));
                    effect.Cleanup();
                }

                if (!hasDespawned && teleportingPawn != null)
                {
                    wasSelected = Find.Selector.SelectedPawns.Any(x => x == teleportingPawn);
                    wasDrafted = teleportingPawn.Drafted;

                    if (teleportingPawn.Spawned)
                    {
                        teleportingPawn.DeSpawn(DestroyMode.Vanish);
                    }


                    if (!innerContainer.TryAdd(teleportingPawn, true))
                    {

                    }

                    hasDespawned = true;
                }
            }
        }

        protected override void Tick()
        {
            int currentTick = Find.TickManager.TicksGame;

            if (hasDespawned && !hasRespawned && currentTick >= spawnTick + delayTicks)
            {
                Teleport();
            }

            if (hasRespawned && currentTick >= spawnTick + delayTicks)
            {
                Destroy();
            }
        }


        protected virtual void Teleport()
        {
            if (teleportingPawn != null && destinationMap != null)
            {
                if (destinationEffecter != null)
                {
                    Effecter effect = destinationEffecter.Spawn();
                    effect.Trigger(new TargetInfo(destinationPosition, destinationMap), new TargetInfo(destinationPosition, destinationMap));
                    effect.Cleanup();
                }

                GenSpawn.Spawn(teleportingPawn, destinationPosition, destinationMap);
                hasRespawned = true;


                if (wasDrafted)
                {
                    teleportingPawn.drafter.Drafted = wasDrafted;
                }

                if (wasSelected)
                {
                    Find.Selector.Select(teleportingPawn, false, true);
                }
            }
        }


        protected virtual void Respawn()
        {
            if (destinationMap == null)
            {
                return;
            }


            this.innerContainer.TryDrop(teleportingPawn, destinationPosition, destinationMap, ThingPlaceMode.Near, out Thing thing, null, null, false);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {

        }

        public static FancyTeleporter Launch(IntVec3 originPos, Map originMap, IntVec3 destPos, Map destMap, Pawn teleportTarget, int delayBetweenRespawn = 200, EffecterDef originEffect = null, EffecterDef destinationEffect = null)
        {
            FancyTeleporter teleporter = (FancyTeleporter)ThingMaker.MakeThing(EMFDefOf.EMF_FancyTeleport);

            teleporter.teleportingPawn = teleportTarget;
            teleporter.originPosition = originPos;
            teleporter.destinationPosition = destPos;
            teleporter.originMap = originMap;
            teleporter.destinationMap = destMap;
            teleporter.delayTicks = delayBetweenRespawn;
            teleporter.originEffecter = originEffect;
            teleporter.destinationEffecter = destinationEffect;

            GenSpawn.Spawn(teleporter, originPos, originMap);

            return teleporter;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(ref teleportingPawn, "teleportingPawn");
            Scribe_Values.Look(ref originPosition, "originPosition");
            Scribe_Values.Look(ref destinationPosition, "destinationPosition");
            Scribe_References.Look(ref originMap, "originMap");
            Scribe_References.Look(ref destinationMap, "destinationMap");
            Scribe_Values.Look(ref delayTicks, "delayTicks", 60); ;
            Scribe_Defs.Look(ref originEffecter, "originEffecter");
            Scribe_Defs.Look(ref destinationEffecter, "destinationEffecter");
            Scribe_Values.Look(ref spawnTick, "spawnTick", -1);
            Scribe_Values.Look(ref hasDespawned, "hasDespawned", false);
            Scribe_Values.Look(ref hasRespawned, "hasRespawned", false);

            Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
            {
              this
            });
        }

    }
}
