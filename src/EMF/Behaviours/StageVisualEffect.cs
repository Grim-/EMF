using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class StageVisualEffect : Thing
    {
        private List<IntVec3> cells;
        private int totalSections;
        private int currentSection = 0;
        private int ticksPerSection = 8;
        private int ticksRemaining;
        private EffecterDef effecterDef;
        private Map map;
        private Action<IntVec3, Map, int> forCellAction;

        public void Initialize(List<IntVec3> cells, Map map, int sections, Action<IntVec3, Map, int> ForCellAction = null, int ticksPerSection = 8)
        {
            this.cells = new List<IntVec3>(cells);
            this.map = map;
            this.totalSections = sections;
            this.forCellAction = ForCellAction;
            this.ticksPerSection = ticksPerSection;
            this.ticksRemaining = ticksPerSection;
        }

        protected override void Tick()
        {
            ticksRemaining--;

            if (ticksRemaining <= 0)
            {
                SpawnEffectsForCurrentSection();
                currentSection++;
                ticksRemaining = ticksPerSection;
                if (currentSection >= totalSections)
                {
                    this.Destroy();
                }
            }
        }

        private void SpawnEffectsForCurrentSection()
        {
            if (cells.NullOrEmpty())
                return;

            int cellsPerSection = Mathf.CeilToInt((float)cells.Count / totalSections);
            int startIndex = currentSection * cellsPerSection;
            int endIndex = Mathf.Min(startIndex + cellsPerSection, cells.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                IntVec3 cell = cells[i];
                if (cell.InBounds(Map))
                {
                    forCellAction?.Invoke(cell, map, currentSection);
                }
            }
        }

        public static StageVisualEffect CreateStageEffect(List<IntVec3> cells, Map map, int sections, Action<IntVec3, Map, int> ForCellAction, int ticksPerSection = 8)
        {
            if (cells.NullOrEmpty())
                return null;

            StageVisualEffect effect = ThingMaker.MakeThing(EMFDefOf.EMF_StagedVisualEffect) as StageVisualEffect;
            if (effect == null)
                return null;

            effect.Initialize(cells, map, sections, ForCellAction, ticksPerSection);
            GenSpawn.Spawn(effect, cells[0], map);
            return effect;
        }



        public static StageVisualEffect CreateConalStageEffect(IntVec3 Origin, IntVec3 Target, int length, float angle, Map map, int sections, Action<IntVec3, Map, int> ForCellAction, int ticksPerSection = 8)
        {
            List<IntVec3> cells = TargetUtil.GetCellsInCone(Origin, Target, length, angle);
            if (cells.NullOrEmpty())
                return null;

            StageVisualEffect effect = ThingMaker.MakeThing(EMFDefOf.EMF_StagedVisualEffect) as StageVisualEffect;
            if (effect == null)
                return null;

            effect.Initialize(cells, map, sections, ForCellAction, ticksPerSection);

            GenSpawn.Spawn(effect, cells[0], map);
            return effect;
        }

        public static StageVisualEffect CreateRadialStageEffect(IntVec3 Origin, float radius, Map map, int sections, Action<IntVec3, Map, int> ForCellAction, int ticksPerSection = 8)
        {
            List<IntVec3> cells = GenRadial.RadialCellsAround(Origin, radius, true).ToList();
            if (cells.NullOrEmpty())
                return null;

            StageVisualEffect effect = ThingMaker.MakeThing(EMFDefOf.EMF_StagedVisualEffect) as StageVisualEffect;
            if (effect == null)
                return null;

            effect.Initialize(cells, map, sections, ForCellAction, ticksPerSection);

            GenSpawn.Spawn(effect, cells[0], map);
            return effect;
        }

        public static StageVisualEffect CreateRectStageEffect(IntVec3 Origin, IntVec3 Target, int width, int length, Map map, int sections, Action<IntVec3, Map, int> ForCellAction, int ticksPerSection = 8)
        {
            List<IntVec3> cells = TargetUtil.GetAllCellsInRect(Origin, Target, width, length).ToList();
            if (cells.NullOrEmpty())
                return null;

            StageVisualEffect effect = ThingMaker.MakeThing(EMFDefOf.EMF_StagedVisualEffect) as StageVisualEffect;
            if (effect == null)
                return null;

            effect.Initialize(cells, map, sections, ForCellAction, ticksPerSection);

            GenSpawn.Spawn(effect, cells[0], map);
            return effect;
        }

        public static StageVisualEffect CreateStageEffect(List<IntVec3> cells, Map map, int sections, EffecterDef effecterDef, int ticksPerSection = 8)
        {
            if (cells.NullOrEmpty())
                return null;

            StageVisualEffect effect = ThingMaker.MakeThing(EMFDefOf.EMF_StagedVisualEffect) as StageVisualEffect;
            if (effect == null)
                return null;

            effect.Initialize(cells, map, sections, (IntVec3 cell, Map targetMap, int sectionIndex) =>
            {
                effecterDef.Spawn(cell, targetMap);
            }, ticksPerSection);

            GenSpawn.Spawn(effect, cells[0], map);
            return effect;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref cells, "cells", LookMode.Value);
            Scribe_Values.Look(ref totalSections, "totalSections");
            Scribe_Values.Look(ref currentSection, "currentSection");
            Scribe_Values.Look(ref ticksPerSection, "ticksPerSection");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
        }
    }
}
