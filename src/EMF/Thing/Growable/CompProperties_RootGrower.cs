using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EMF
{
    public class CompProperties_RootGrower : CompProperties
    {
        public ThingDef rootDef;
        public int rootGrowthInterval = 250;
        public int maxRootDistance = 10;

        public CompProperties_RootGrower()
        {
            compClass = typeof(Comp_RootGrower);
        }
    }

    public class Comp_RootGrower : ThingComp
    {
        private int rootGrowthTick = 0;
        private List<Thing> placedRoots = new List<Thing>();
        private HashSet<IntVec3> visitedPositions = new HashSet<IntVec3>();
        private IntVec3 currentDirection = IntVec3.Zero;
        private Queue<IntVec3> recentPositions = new Queue<IntVec3>();
        private int positionMemoryLength = 5;

        private CompProperties_RootGrower Props => (CompProperties_RootGrower)props;

        private bool IsGrowing = true;

        public override void CompTick()
        {
            base.CompTick();

            if (!IsGrowing)
                return;

            rootGrowthTick++;
            if (rootGrowthTick >= Props.rootGrowthInterval)
            {
                rootGrowthTick = 0;
                TryGrowRoot();
            }
        }

        public override void Notify_SignalReceived(Signal signal)
        {
            base.Notify_SignalReceived(signal);

            if (signal.tag == "RootGrower.Start")
            {
                EnableRootGrowing();
            }

            if (signal.tag == "RootGrower.Stop")
            {
                StopRootGrowing();
            }
        }

        public void EnableRootGrowing()
        {
            IsGrowing = true;
            if (placedRoots.Count == 0)
            {
                List<IntVec3> directions = GenAdj.CardinalDirections.ToList();
                directions.Shuffle();
                currentDirection = directions.First();
            }
        }

        public void StopRootGrowing()
        {
            IsGrowing = false;
        }


        private void TryGrowRoot()
        {
            placedRoots.RemoveAll(r => r == null || r.Destroyed);

            IntVec3 sourcePos = parent.Position;
            visitedPositions.Add(sourcePos);

            if (placedRoots.Count > 0)
            {
                sourcePos = placedRoots.Last().Position;
            }

            List<IntVec3> potentialDirections = GetPrioritizedDirections();

            foreach (IntVec3 offset in potentialDirections)
            {
                IntVec3 targetPos = sourcePos + offset;

                // Skip if we've already visited this position
                if (visitedPositions.Contains(targetPos))
                    continue;

                // Basic validity checks
                if (!IsValidGrowthLocation(targetPos))
                    continue;

                // Create and place the root
                Thing root = ThingMaker.MakeThing(Props.rootDef);
                Thing placedRoot = GenSpawn.Spawn(root, targetPos, parent.Map);

                if (placedRoot != null)
                {
                    placedRoots.Add(placedRoot);
                    visitedPositions.Add(targetPos);

                    currentDirection = offset;

                    recentPositions.Enqueue(targetPos);
                    if (recentPositions.Count > positionMemoryLength)
                        recentPositions.Dequeue();

                    FleckMaker.ThrowDustPuff(targetPos, parent.Map, 0.5f);
                    return;
                }
            }

            HandleGrowthDeadEnd();
        }

        private List<IntVec3> GetPrioritizedDirections()
        {
            List<IntVec3> result = new List<IntVec3>();

            if (currentDirection != IntVec3.Zero)
            {
                result.Add(currentDirection);

                if (currentDirection.x != 0)
                {
                    result.Add(new IntVec3(currentDirection.x, 0, 1));
                    result.Add(new IntVec3(currentDirection.x, 0, -1));
                }
                else if (currentDirection.z != 0)
                {
                    result.Add(new IntVec3(1, 0, currentDirection.z));
                    result.Add(new IntVec3(-1, 0, currentDirection.z));
                }
            }

            // Add all other directions
            foreach (IntVec3 dir in GenAdj.CardinalDirections)
            {
                if (!result.Contains(dir))
                {
                    result.Add(dir);
                }
            }

            // result.Shuffle();

            return result;
        }

        private bool IsValidGrowthLocation(IntVec3 pos)
        {
            if (!pos.InBounds(parent.Map))
                return false;

            if ((pos - parent.Position).LengthHorizontalSquared > Props.maxRootDistance * Props.maxRootDistance)
                return false;

            TerrainDef terrain = parent.Map.terrainGrid.TerrainAt(pos);
            if (terrain.passability == Traversability.Impassable)
                return false;

            foreach (Thing thing in pos.GetThingList(parent.Map))
            {
                if (thing.def == Props.rootDef)
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleGrowthDeadEnd()
        {
            if (recentPositions.Count > 0)
            {
                IntVec3 backtrackPos = recentPositions.Peek();

                List<IntVec3> newDirections = GenAdj.CardinalDirections.ToList();
                newDirections.Shuffle();

                currentDirection = newDirections.First();

                recentPositions.Clear();
            }
            else
            {
                // Reset and try a completely new direction
                List<IntVec3> directions = GenAdj.CardinalDirections.ToList();
                directions.Shuffle();
                currentDirection = directions.First();
            }
        }

        public void DestroyRoots()
        {
            foreach (Thing root in placedRoots)
            {
                if (root != null && !root.Destroyed)
                {
                    root.Destroy();
                }
            }

            placedRoots.Clear();
            visitedPositions.Clear();
            recentPositions.Clear();
            currentDirection = IntVec3.Zero;
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            DestroyRoots();
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (mode == DestroyMode.KillFinalize)
            {
                DestroyRoots();
            }
        }

        public override string CompInspectStringExtra()
        {
            if (placedRoots.Count > 0)
            {
                return "Root network: " + placedRoots.Count + " roots" + (IsGrowing ? " (Growing)" : " (Dormant)");
            }
            return null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref rootGrowthTick, "rootGrowthTick", 0);
            Scribe_Values.Look(ref IsGrowing, "isGrowing", false);
            Scribe_Collections.Look(ref placedRoots, "placedRoots", LookMode.Reference);
            Scribe_Values.Look(ref currentDirection, "currentDirection", IntVec3.Zero);

            // Save visited positions
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<IntVec3> visitedPositionsList = visitedPositions.ToList();
                Scribe_Collections.Look(ref visitedPositionsList, "visitedPositions", LookMode.Value);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<IntVec3> visitedPositionsList = null;
                Scribe_Collections.Look(ref visitedPositionsList, "visitedPositions", LookMode.Value);
                if (visitedPositionsList != null)
                {
                    visitedPositions = new HashSet<IntVec3>(visitedPositionsList);
                }
                else
                {
                    visitedPositions = new HashSet<IntVec3>();
                }
            }
        }
    }


}