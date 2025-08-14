using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ActiveZoneDef : ThingDef
    {
        public int ZoneLifeTime = 1000;
        public Color zoneColor = Color.white;

        public ActiveZoneDef()
        {
            thingClass = typeof(ActiveZone);
        }
    }

    public class ActiveZone : ThingWithComps
    {
        public List<IntVec3> ZoneCells = new List<IntVec3>();
        protected List<ActiveZoneComp> ZoneComps => this.GetComps<ActiveZoneComp>().ToList();
        public ActiveZoneDef ActiveZoneDef => (ActiveZoneDef)def;

        public int ZoneLifeTime = 1000;
        protected int ZoneLifetimeTicks = 0;
        protected DamageDef DamageDef;
        protected FloatRange Damage;
        public int ticksBetweenDamage = 100;
        public int maxTargets = -1;
        public EffecterDef targetDamageEffecterDef = null;

        protected HashSet<Thing> previousThingsInZone = new HashSet<Thing>();
        public HashSet<Thing> ThingsInZoneRead => new HashSet<Thing>(previousThingsInZone);
        public HashSet<Pawn> PawnsInZoneRead => new HashSet<Pawn>(ThingsInZoneRead.Where(x=>  x is Pawn).Cast<Pawn>().ToHashSet());


        protected Thing owner = null;
        public Thing Owner => owner;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);



            if (ZoneCells == null || ZoneCells.Empty())
            {
                return;
            }

            foreach (var item in ZoneComps)
            {
                item.OnZoneSpawned(this, ref ZoneCells);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (ZoneCells == null || ZoneCells.Empty())
            {
                base.DeSpawn(mode);
                return;
            }

            foreach (var item in ZoneComps)
            {
                item.OnZoneDespawned(this, ref ZoneCells);
            }


            Log.Message("zone despawned");
            base.DeSpawn(mode);
        }

        public void SetZoneCells(List<IntVec3> cells)
        {
            ZoneCells = cells.ToList();
        }

        public void SetOwner(Thing owner)
        {
            this.owner = owner;
        }

        public void SetDamage(DamageDef damageDef, FloatRange damage)
        {
            DamageDef = damageDef;
            Damage = damage;
        }

        protected override void Tick()
        {
            base.Tick();

            if (ZoneCells == null || ZoneCells.Empty())
            {
                return;
            }
            GenDraw.DrawFieldEdges(ZoneCells, ActiveZoneDef.zoneColor);
            ZoneLifetimeTicks++;

            if (ZoneLifetimeTicks >= ZoneLifeTime)
            {
                if (!this.Destroyed)
                {
                    this.Destroy();
                    return;
                }
            }

            if (previousThingsInZone.Count > 0)
            {
                foreach (var previousThing in previousThingsInZone.ToArray())
                {
                    if (this.ZoneCells.Contains(previousThing.Position))
                    {
                        continue;
                    }
                    else
                    {
                        OnThingLeftZone(previousThing);
                        previousThingsInZone.Remove(previousThing);
                    }                   
                }
            }

            HashSet<Thing> currentThingsInZone = GetCurrentThingsInZone(ref ZoneCells);

            foreach (var thing in currentThingsInZone)
            {
                if (previousThingsInZone.Contains(thing))
                {
                    continue;
                }
                else
                {
                    OnThingEnteredZone(thing);
                    previousThingsInZone.Add(thing);
                }

            }

            foreach (var item in ZoneComps)
            {
                item.OnZoneTick(this, ref ZoneCells);
            }
        }

        protected virtual void OnThingEnteredZone(Thing thing)
        {
           // Log.Message($"{thing.Label} entered zone");
            foreach (var comp in ZoneComps)
            {
                comp.OnThingEnteredZone(this, thing);
            }
        }

        protected virtual void OnThingLeftZone(Thing thing)
        {
            //Log.Message($"{thing.Label} left zone");
            foreach (var comp in ZoneComps)
            {
                comp.OnThingLeftZone(this, thing);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (ZoneCells == null || ZoneCells.Empty())
            {
                return;
            }

         
        }

        public HashSet<Thing> GetCurrentThingsInZone(ref List<IntVec3> cells)
        {
            return TargetUtil.GetThingsInCells(cells, this.Map, (Thing thing) =>
            {
                return thing.def.selectable;
            });
        }

        public static ActiveZone SpawnZone(ActiveZoneDef activeZoneDef, IntVec3 SpawnPosition, List<IntVec3> ZoneCells, Map map, Thing zoneOwner)
        {
            if (activeZoneDef == null)
            {
                return null;
            }

            if (ZoneCells.NullOrEmpty())
            {
                return null;
            }

            ActiveZone zone = (ActiveZone)ThingMaker.MakeThing(activeZoneDef);
            zone.ZoneLifeTime = activeZoneDef.ZoneLifeTime;
            zone.SetZoneCells(ZoneCells);
            zone.SetOwner(zoneOwner);
            zone = (ActiveZone)GenSpawn.Spawn(zone, SpawnPosition, map);
            return zone;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref ZoneCells, "ZoneCells", LookMode.Reference);
            Scribe_Values.Look(ref ZoneLifetimeTicks, "ZoneLifetimeTicks");
            Scribe_Defs.Look(ref DamageDef, "DamageDef");
            Scribe_Values.Look(ref Damage, "Damage");
            Scribe_References.Look(ref owner, "Owner");
            Scribe_Collections.Look(ref previousThingsInZone, "previousThingsInZone", LookMode.Reference);
        }
    }





}