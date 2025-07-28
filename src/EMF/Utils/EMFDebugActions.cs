using LudeonTK;
using RimWorld;
using Verse;

namespace EMF
{
    public static class EMFDebugActions
    {
        [DebugAction("Magic And Myth", "Petrify Pawn", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
    }
}
