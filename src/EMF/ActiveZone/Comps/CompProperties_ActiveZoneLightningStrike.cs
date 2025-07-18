using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_ActiveZoneLightningStrike : CompProperties
    {
        public int ticksBetweenStrikes = 500;
        public int maxTargets = -1;
        public float radius = 3f;

        public bool targetEnemies = true;

        public bool edgesOnly = true;

        public FloatRange damage = new FloatRange(1, 1);
        public DamageDef damageDef;

        public EffecterDef targetDamageEffecterDef = null;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();
        public CompProperties_ActiveZoneLightningStrike()
        {
            compClass = typeof(ActiveZoneComp_LightningStrike);
        }
    }

    public class ActiveZoneComp_LightningStrike : ActiveZoneComp
    {
        CompProperties_ActiveZoneLightningStrike Props => (CompProperties_ActiveZoneLightningStrike)props;

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);
            if (ParentZone.IsHashIntervalTick(Props.ticksBetweenStrikes))
            {
                if (Props.targetEnemies)
                {
                    Pawn target = (Pawn)ParentZone.ThingsInZoneRead.Where(x => x.CanTargetThing(this.ParentZone.Faction, Props.friendlyFireSettings) && x is Pawn).RandomElement();

                    if (target != null)
                    {
                        GenerateStrikeAt(target.Position);
                    }
                }
                else
                {
                    if (Props.edgesOnly)
                    {
                        List<IntVec3> edgeCells = GetEdgeCells(cells);
                        if (edgeCells.Any())
                        {

                            foreach (var item in edgeCells)
                            {
                                if (Rand.Value > 0.6f)
                                {
                                    GenerateStrikeAt(item);
                                }
                            }

                        }
                    }
                    else
                    {
                        GenerateStrikeAt(cells.RandomElement());
                    }
                }

            }
        }

        private void GenerateStrikeAt(IntVec3 cell)
        {
            LightningStrike.GenerateLightningStrike(ParentZone.Map, cell, Props.radius, (IntVec3 Cell, Map map) =>
            {
                if (Cell.GetDamageablePawn(map, this.ParentZone.Faction, Props.friendlyFireSettings, out Pawn pawn))
                {
                    pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange));
                }
            }, 1);
        }

        private List<IntVec3> GetEdgeCells(List<IntVec3> zoneCells)
        {
            List<IntVec3> edgeCells = new List<IntVec3>();
            HashSet<IntVec3> zoneCellsSet = new HashSet<IntVec3>(zoneCells);

            foreach (IntVec3 cell in zoneCells)
            {
                bool isEdge = false;
                foreach (IntVec3 neighbor in GenAdjFast.AdjacentCells8Way(cell))
                {
                    if (!zoneCellsSet.Contains(neighbor))
                    {
                        isEdge = true;
                        break;
                    }
                }

                if (isEdge)
                {
                    edgeCells.Add(cell);
                }
            }

            return edgeCells;
        }
    }


}
