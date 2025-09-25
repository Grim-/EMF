using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EMF
{
    public class CompProperties_AbilityEffectDomainExpansion : CompProperties_AbilityEffectChannelable
    {
        public ActiveZoneDef domainZoneDef;
        public float domainRadius = 10f;
        public bool useCustomShape = false;
        public List<IntVec2> customShapeOffsets = new List<IntVec2>();
        public bool centerOnCaster = false;
        public bool moveWithCaster = false;
        public int expansionSpeed = 0;
        public SoundDef domainStartSound;
        public SoundDef domainEndSound;
        public FleckDef domainFleckDef;

        public CompProperties_AbilityEffectDomainExpansion()
        {
            compClass = typeof(CompAbilityEffectDomainExpansion);
        }
    }

    public class CompAbilityEffectDomainExpansion : CompAbilityEffectChannelable, IChannelableAbility
    {
        CompProperties_AbilityEffectDomainExpansion Props => (CompProperties_AbilityEffectDomainExpansion)props;

        private ActiveZone spawnedDomain;
        private IntVec3 domainCenter;
        private List<IntVec3> fullDomainCells;
        private int currentExpansionTick = 0;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Log.Message($"Apply");


            Job newJob = JobMaker.MakeJob(EMFDefOf.EMF_CastAbilityChannel, this.parent.pawn);
            newJob.ability = this.parent;
            this.parent.pawn.jobs.StartJob(newJob, Verse.AI.JobCondition.Succeeded);
        }

        public override void OnChanneBegin(LocalTargetInfo target)
        {
            base.OnChanneBegin(target);
            Log.Message($"OnChanneBegin");
            if (Props.domainZoneDef == null)
            {
                Log.Error($"Domain expansion ability {parent.def.defName} has no domainZoneDef set");
                ShouldCancel = true;
                return;
            }

            domainCenter = Props.centerOnCaster ? parent.pawn.Position : target.Cell;

            if (!domainCenter.InBounds(parent.pawn.Map))
            {
                ShouldCancel = true;
                return;
            }

            fullDomainCells = GetDomainCells(domainCenter);

            if (fullDomainCells.NullOrEmpty())
            {
                ShouldCancel = true;
                return;
            }

            List<IntVec3> initialCells = fullDomainCells;

            spawnedDomain = ActiveZone.SpawnZone(
                Props.domainZoneDef,
                domainCenter,
                initialCells,
                parent.pawn.Map,
                parent.pawn
            );
            Log.Message($"SPAWNED DOMAIN {spawnedDomain}");
            if (spawnedDomain == null)
            {
                Log.Error($"Failed to spawn domain zone for {parent.def.defName}");
                ShouldCancel = true;
                return;
            }

            spawnedDomain.ZoneLifeTime = int.MaxValue;

            if (Props.domainStartSound != null)
            {
                // Props.domainStartSound.PlayOneShot(SoundInfo.InMap(domainCenter, parent.pawn.Map));
            }

            if (Props.domainFleckDef != null)
            {
                foreach (var cell in initialCells.Take(10))
                {
                    FleckMaker.Static(cell.ToVector3Shifted(), parent.pawn.Map, Props.domainFleckDef);
                }
            }

            currentExpansionTick = 0;
        }

        public override void OnChannelTick(LocalTargetInfo target)
        {
            base.OnChannelTick(target);

            //if (spawnedDomain == null || spawnedDomain.Destroyed)
            //{
            //    ShouldCancel = true;
            //    return;
            //}

            //if (!parent.pawn.Spawned || parent.pawn.Dead || parent.pawn.Downed)
            //{
            //    ShouldCancel = true;
            //    return;
            //}

            //if (Props.moveWithCaster && parent.pawn.Position != domainCenter)
            //{
            //    IntVec3 offset = parent.pawn.Position - domainCenter;
            //    domainCenter = parent.pawn.Position;

            //    List<IntVec3> newCells = GetDomainCells(domainCenter);

            //    if (Props.expansionSpeed > 0)
            //    {
            //        newCells = GetExpandedCells(currentExpansionTick);
            //    }

            //    spawnedDomain.SetZoneCells(newCells);
            //}

            //if (Props.expansionSpeed > 0 && currentExpansionTick < Props.expansionSpeed)
            //{
            //    currentExpansionTick++;
            //    List<IntVec3> expandedCells = GetExpandedCells(currentExpansionTick);
            //    spawnedDomain.SetZoneCells(expandedCells);

            //    if (Props.domainFleckDef != null && currentExpansionTick % 10 == 0)
            //    {
            //        var newCells = expandedCells.Except(GetExpandedCells(currentExpansionTick - 1)).Take(5);
            //        foreach (var cell in newCells)
            //        {
            //            FleckMaker.Static(cell.ToVector3Shifted(), parent.pawn.Map, Props.domainFleckDef);
            //        }
            //    }
            //}

            //if (parent.pawn.IsHashIntervalTick(60))
            //{
            //    if (!CanMaintainConcentration())
            //    {
            //        ShouldCancel = true;
            //    }
            //}
        }

        public override void OnChanneEnd(LocalTargetInfo target)
        {
            base.OnChanneEnd(target);

            if (spawnedDomain != null && !spawnedDomain.Destroyed)
            {
                if (Props.domainEndSound != null)
                {
                    // Props.domainEndSound.PlayOneShot(SoundInfo.InMap(spawnedDomain.Position, spawnedDomain.Map));
                }

                if (Props.domainFleckDef != null)
                {
                    foreach (var cell in spawnedDomain.ZoneCells.Take(10))
                    {
                        FleckMaker.Static(cell.ToVector3Shifted(), spawnedDomain.Map, Props.domainFleckDef);
                    }
                }

                spawnedDomain.Destroy();
                spawnedDomain = null;
            }

            fullDomainCells = null;
            currentExpansionTick = 0;
        }

        private List<IntVec3> GetDomainCells(IntVec3 center)
        {
            return GenRadial.RadialCellsAround(center, Props.domainRadius, true)
                    .Where(c => c.InBounds(parent.pawn.Map))
                    .ToList();
        }

        private List<IntVec3> GetExpandedCells(int expansionTick)
        {
            if (Props.expansionSpeed <= 0 || fullDomainCells.NullOrEmpty())
            {
                return fullDomainCells;
            }

            float expansionProgress = Mathf.Clamp01((float)expansionTick / Props.expansionSpeed);
            float currentRadius = Props.domainRadius * expansionProgress;

            return GenRadial.RadialCellsAround(domainCenter, currentRadius, true)
                   .Where(c => c.InBounds(parent.pawn.Map))
                   .ToList();
        }

        private bool CanMaintainConcentration()
        {
            //if (parent.pawn.stances != null && parent.pawn.stances.stunner.Stunned)
            //{
            //    return false;
            //}

            //if (parent.pawn.needs?.rest != null && parent.pawn.needs.rest.CurCategory >= RestCategory.VeryTired)
            //{
            //    return false;
            //}

            //if (parent.pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < 0.3f)
            //{
            //    return false;
            //}

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref spawnedDomain, "spawnedDomain");
            Scribe_Values.Look(ref domainCenter, "domainCenter");
            Scribe_Collections.Look(ref fullDomainCells, "fullDomainCells", LookMode.Value);
            Scribe_Values.Look(ref currentExpansionTick, "currentExpansionTick");
        }
    }
}
