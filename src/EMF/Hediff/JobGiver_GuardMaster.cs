using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EMF
{
    public class JobGiver_GuardMaster : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {

            if (!pawn.IsASummon(out HediffComp_Summon summonComp))
            {
                return null;
            }

            var master = summonComp.Summoner;

            if (master.Dead || !master.Spawned || master.Map != pawn.Map)
            {
                return null;
            }

            var enemies = GetNearbyThreats(pawn, master);
            if (enemies.Count > 0)
            {
                var target = GetPriorityTarget(enemies, pawn, master);
                if (target != null && pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                {
                    var job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                    job.maxNumMeleeAttacks = 10;
                    job.expiryInterval = 500;
                    return job;
                }
            }

            if (pawn.Position.DistanceTo(master.Position) > 20f)
            {
                var followSpot = GetFollowPosition(master, pawn, 20f);
                if (followSpot.IsValid && pawn.CanReach(followSpot, PathEndMode.OnCell, Danger.Deadly))
                {
                    return JobMaker.MakeJob(JobDefOf.Goto, followSpot);
                }
            }

            return null;
        }

        private List<Thing> GetNearbyThreats(Pawn guardian, Pawn master)
        {
            var threats = new List<Thing>();
            var searchRadius = 30f;

            foreach (var thing in GenRadial.RadialDistinctThingsAround(master.Position, master.Map, searchRadius, true))
            {
                if (thing is Pawn enemy && IsHostileTo(enemy, master))
                {
                    threats.Add(enemy);
                }
            }

            return threats;
        }

        private bool IsHostileTo(Pawn enemy, Pawn master)
        {
            if (enemy == null || enemy.Dead || enemy.Downed) return false;
            if (enemy.Faction == null) return enemy.HostileTo(master);
            return enemy.Faction.HostileTo(master.Faction);
        }

        private Thing GetPriorityTarget(List<Thing> enemies, Pawn guardian, Pawn master)
        {
            Thing bestTarget = null;
            float bestScore = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy is Pawn enemyPawn)
                {
                    float distToMaster = enemy.Position.DistanceTo(master.Position);
                    float distToGuardian = enemy.Position.DistanceTo(guardian.Position);
                    float threatLevel = enemyPawn.kindDef.combatPower;

                    float score = (distToMaster * 2f) + distToGuardian - (threatLevel * 0.1f);

                    if (enemyPawn.CurJobDef == JobDefOf.AttackMelee && enemyPawn.CurJob?.targetA.Thing == master)
                    {
                        score -= 100f;
                    }

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestTarget = enemy;
                    }
                }
            }

            return bestTarget;
        }

        private IntVec3 GetFollowPosition(Pawn master, Pawn guardian, float followDistance)
        {
            var offset = (guardian.Position - master.Position).ToVector3().normalized * followDistance;
            var targetPos = master.Position + offset.ToIntVec3();

            if (targetPos.InBounds(master.Map) && targetPos.Standable(master.Map))
            {
                return targetPos;
            }

            for (int i = 0; i < 8; i++)
            {
                var pos = master.Position + GenAdj.AdjacentCells[i];
                if (pos.InBounds(master.Map) && pos.Standable(master.Map))
                {
                    return pos;
                }
            }

            return master.Position;
        }
    }
}