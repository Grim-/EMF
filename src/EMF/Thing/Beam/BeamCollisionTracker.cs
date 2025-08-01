using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class BeamCollisionTracker
    {
        private Beam_Thing parent;
        private HashSet<IntVec3> cachedBeamPath = new HashSet<IntVec3>();

        public BeamCollisionTracker(Beam_Thing beam)
        {
            this.parent = beam;
        }


        public bool CheckForCollisions()
        {
            if (parent.parameters != null && parent.parameters.explodeOnImpact)
            {
                if (CheckForObstacleInPath(parent.travelProgress - parent.TravelSpeed, parent.travelProgress, out Thing thing) && thing.HostileTo(parent.sourceAbility.pawn))
                {
                    return true;
                }
            }

            return false;
        }


        public bool CheckForObstacleInPath(float startDistance, float endDistance, out Thing obstacle)
        {
            obstacle = null;
            List<Thing> list = new List<Thing>();
            for (float num = startDistance; num <= endDistance; num += 0.5f)
            {
                Vector3 vect = parent.Origin + parent.CurrentDirection * num;
                IntVec3 c = vect.ToIntVec3();

                if (c.InBounds(parent.Map))
                {
                    List<Thing> thingList = c.GetThingList(parent.Map);
                    foreach (Thing thing in thingList)
                    {
                        if (parent.CanDamage(thing))
                        {
                            if (IsObstacle(thing))
                            {
                                list.Add(thing);
                            }
                        }
                    }
                }
            }
            obstacle = (from t in list
                        orderby Vector3.Distance(parent.Origin, t.DrawPos)
                        select t).FirstOrDefault<Thing>();
            return obstacle != null;
        }
        public bool IsObstacle(Thing thing)
        {
            if (!(thing is Pawn))
            {
                Building building = thing as Building;
                if (building == null || building.def.Fillage != FillCategory.Full)
                {
                    return thing.def.mineable;
                }
            }
            return true;
        }

        public HashSet<IntVec3> GetCellsAlongBeam()
        {
            HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
            HashSet<IntVec3> result;
            if (parent.Map == null)
            {
                result = hashSet;
            }
            else
            {
                float num = Vector3.Distance(parent.Origin.Yto0(), parent.currentEndpoint.Yto0());
                for (float num2 = 0f; num2 < num; num2 += 0.5f)
                {
                    Vector3 vect = Vector3.MoveTowards(parent.Origin, parent.currentEndpoint, num2);
                    IntVec3 intVec = vect.ToIntVec3();
                    bool flag2 = !intVec.InBounds(parent.Map);
                    if (!flag2)
                    {
                        bool flag3 = hashSet.Add(intVec);
                        if (flag3)
                        {
                            IntVec3 center = intVec;
                            BeamParameters beamParameters = parent.parameters;
                            foreach (IntVec3 intVec2 in GenRadial.RadialCellsAround(center, parent.BeamWidth, true))
                            {
                                bool flag4 = intVec2.InBounds(parent.Map);
                                if (flag4)
                                {
                                    hashSet.Add(intVec2);
                                }
                            }
                        }
                    }
                }

                if (parent.currentEndpoint.ToIntVec3().InBounds(parent.Map))
                {
                    foreach (IntVec3 currentCell in GenRadial.RadialCellsAround(parent.currentEndpoint.ToIntVec3(), parent.ExplosionRadius, true))
                    {
                        if (currentCell.InBounds(parent.Map))
                        {
                            hashSet.Add(currentCell);
                        }
                    }
                }
                result = (from c in hashSet
                          where c.DistanceTo(parent.caster.Position) >= 2f
                          select c).ToHashSet<IntVec3>();
            }
            return result;
        }

        public HashSet<IntVec3> GetCellsAlongSegments(List<BeamSegment> segments)
        {
            HashSet<IntVec3> allCells = new HashSet<IntVec3>();

            foreach (BeamSegment segment in segments)
            {
                float segmentLength = Vector3.Distance(segment.startPoint, segment.endPoint);
                for (float step = 0f; step < segmentLength; step += 0.5f)
                {
                    Vector3 point = Vector3.MoveTowards(segment.startPoint, segment.endPoint, step);
                    IntVec3 cell = point.ToIntVec3();

                    if (cell.InBounds(parent.Map))
                    {
                        allCells.Add(cell);

                        foreach (IntVec3 nearbyCell in GenRadial.RadialCellsAround(cell, parent.BeamWidth, true))
                        {
                            if (nearbyCell.InBounds(parent.Map))
                            {
                                allCells.Add(nearbyCell);
                            }
                        }
                    }
                }
            }

            return allCells;
        }

        public void UpdateThingsInBeamPath(HashSet<Thing> thingsInBeamPath, List<BeamSegment> segments)
        {
            thingsInBeamPath.Clear();

            if (parent.Map != null)
            {
                HashSet<IntVec3> impactCells = this.GetCellsAlongSegments(segments);
                foreach (IntVec3 c in impactCells)
                {
                    if (c.InBounds(parent.Map))
                    {
                        List<Thing> thingList = c.GetThingList(parent.Map);
                        if (thingList != null)
                        {
                            thingsInBeamPath.UnionWith(from t in thingList
                                                       where t != null && t != parent && t != parent.caster
                                                       select t);
                        }
                    }
                }
                GenDraw.DrawFieldEdges(impactCells.ToList(), 2900);
            }
        }
    }
}