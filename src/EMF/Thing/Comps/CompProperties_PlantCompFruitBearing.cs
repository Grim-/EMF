using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_PlantCompFruitBearing : CompProperties
    {
        public List<DropItemChance> possibleDrops = new List<DropItemChance>();
        public IntRange dropCount = new IntRange(1, 1);
        public float dropChance = 1f;

        public CompProperties_PlantCompFruitBearing()
        {
            compClass = typeof(PlantComp_FruitBearing);
        }
    }


    public class PlantComp_FruitBearing : ThingComp
    {
        protected int currentIntervals = 0;
        protected int intervalsUntilNextSpawn = 10;
        protected CompProperties_PlantCompFruitBearing Props => (CompProperties_PlantCompFruitBearing)props;
        protected Plant ParentPlant => this.parent as Plant;

        public override void CompTick()
        {
            base.CompTick();
            if (ParentPlant == null)
            {
                return;
            }

            if (this.parent.IsHashIntervalTick(2400))
            {
                currentIntervals++;
                if (currentIntervals >= intervalsUntilNextSpawn)
                {
                    currentIntervals = 0;
                    if (Rand.Value <= Props.dropChance && ParentPlant.Growth >= 0.6f)
                    {
                        int numToDrop = Props.dropCount.RandomInRange;
                        IntVec3 spawnLoc = CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, 2);

                        if (spawnLoc.IsValid)
                        {
                            List<DropItemChance> selectedDrops = Props.possibleDrops
                                .OrderBy(x => Rand.Value)
                                .Take(numToDrop)
                                .ToList();

                            foreach (DropItemChance drop in selectedDrops)
                            {
                                Thing thing = ThingMaker.MakeThing(drop.thingDef);
                                thing.stackCount = drop.count.RandomInRange;
                                GenSpawn.Spawn(thing, spawnLoc, parent.Map);
                            }
                        }
                    }
                }
            }
        }
    }
}
