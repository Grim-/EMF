using Verse;

namespace EMF
{
    public abstract class BaseEvolutionConditionWorker
    {
        public PawnKindDef newKind;
        public EvolutionConditionProperties props;

        public abstract bool MeetsCriteria(Pawn pawn);
        public abstract string GetFailureReason(Pawn pawn);
    }
}