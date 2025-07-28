using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public static class EMFDebugActions
    {
        [DebugAction("EMF Utils", "Petrify Pawn", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void PetrifyPawn()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetHumans = true,
                canTargetMechs = true,
                mapObjectTargetsMustBeAutoAttackable = false
            },
            (LocalTargetInfo target) =>
            {
                if (target.Thing != null && target.Thing is Pawn pawn)
                {
                    Map pawnMap = pawn.Map;
                    IntVec3 position = pawn.Position;
                    PetrifiedStatue.PetrifyPawn(
                        EMFDefOf.EMF_PetrifiedStatue,
                        pawn,
                        position,
                        pawnMap
                    );
                    Messages.Message("Petrified " + pawn.LabelShort, MessageTypeDefOf.NeutralEvent);
                }
            });
        }


        [DebugAction("EMF Utils", "Spawn in grid", false, false, false, false, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
        private static List<DebugActionNode> SetTerrainRect()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();
            foreach (ThingDef localDef2 in DefDatabase<ThingDef>.AllDefs)
            {
                ThingDef localDef = localDef2;
                if (localDef2.BuildableByPlayer)
                {
                    list.Add(new DebugActionNode(localDef.defName, DebugActionType.Action, () =>
                    {
                        ThingDef defName = localDef;

                        DebugToolsGeneral.GenericRectTool(defName.defName, (CellRect cellRect) =>
                        {
                            IntVec2 sizePerCell = defName.Size;
                            int stepX = sizePerCell.x + 1;
                            int stepZ = sizePerCell.z + 1;

                            for (int x = cellRect.minX; x + sizePerCell.x <= cellRect.maxX + 1; x += stepX)
                            {
                                for (int z = cellRect.minZ; z + sizePerCell.z <= cellRect.maxZ + 1; z += stepZ)
                                {
                                    IntVec3 spawnPos = new IntVec3(x, 0, z);
                                    if (cellRect.Contains(spawnPos))
                                    {
                                        Thing thing = ThingMaker.MakeThing(defName, defName.MadeFromStuff ? ThingDefOf.Steel : null);
                                        thing.SetFaction(Faction.OfPlayer);
                                        GenSpawn.Spawn(thing, spawnPos, Find.CurrentMap);
                                    }
                                }
                            }
                        });
                    }));
                }
            }
            return list;
        }
    }


}
