using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class GraphicConditional : DefModExtension
    {
        public List<GraphicConditionalData> conditionalGraphics;
        public GraphicConditionWorker GetWorker(GraphicConditionalData data)
        {
            GraphicConditionWorker worker = (GraphicConditionWorker)Activator.CreateInstance(data.workerClass);
            worker.data = data;
            return worker;
        }
    }

    public class GraphicConditionalData
    {
        public GraphicData graphicData;
        public Type workerClass;
        public int priority = 0;
    }

    public abstract class GraphicConditionWorker
    {
        public GraphicConditionalData data;

        public abstract bool CanApply(Pawn pawn);
    }

    public static class PawnGraphicConditionalUtility
    {
        public static bool TryGetConditionalGraphic(Pawn pawn, out GraphicConditionalData graphicData)
        {
            graphicData = null;

            var extension = pawn.def.GetModExtension<GraphicConditional>();
            if (extension?.conditionalGraphics == null)
                return false;

            var validGraphics = extension.conditionalGraphics
                .Where(g => g != null && g.workerClass != null)
                .Where(g => extension.GetWorker(g).CanApply(pawn))
                .OrderByDescending(g => g.priority);

            graphicData = validGraphics.FirstOrDefault();
            return graphicData != null;
        }
    }

    public class PawnRenderNodeWorker_AnimalBody_Conditional : PawnRenderNodeWorker_AnimalBody
    {
        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            if (PawnGraphicConditionalUtility.TryGetConditionalGraphic(parms.pawn, out GraphicConditionalData conditionalData))
            {
                return GetGraphicFromConditionalData(parms.pawn, conditionalData);
            }
            return base.GetGraphic(node, parms);
        }

        private Graphic GetGraphicFromConditionalData(Pawn pawn, GraphicConditionalData data)
        {
            return data.graphicData.GraphicColoredFor(pawn);
        }
    }

    public class GraphicConditionalData_Season : GraphicConditionalData
    {
        public Season season = Season.Spring;
    }

    public class GraphicConditionWorker_Season : GraphicConditionWorker
    {
        GraphicConditionalData_Season Data => (GraphicConditionalData_Season)data;

        public override bool CanApply(Pawn pawn)
        {
            if (pawn.Map == null)
                return false;
            Vector2 longLat = Find.WorldGrid.LongLatOf(pawn.Map.Tile.tileId);
            Season currentSeason = GenDate.Season((long)Find.TickManager.TicksGame, longLat);
            return currentSeason == Data.season;
        }
    }
}