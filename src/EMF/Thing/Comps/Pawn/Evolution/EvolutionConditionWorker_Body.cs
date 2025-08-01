using Verse;

namespace EMF
{    // Body properties
    public class BodyEvolutionProperties : EvolutionConditionProperties
    {
        public float minBodySize = 0f;
        public float maxBodySize = float.MaxValue;
        public Gender? requiredGender = null;
    }
    public class EvolutionConditionWorker_Body : BaseEvolutionConditionWorker
    {
        public override bool MeetsCriteria(Pawn pawn)
        {
            var bodyProps = props as BodyEvolutionProperties;
            if (bodyProps == null) return true;

            if (pawn.BodySize < bodyProps.minBodySize || pawn.BodySize > bodyProps.maxBodySize)
                return false;

            if (bodyProps.requiredGender.HasValue && pawn.gender != bodyProps.requiredGender.Value)
                return false;

            return true;
        }

        public override string GetFailureReason(Pawn pawn)
        {
            var bodyProps = props as BodyEvolutionProperties;
            if (bodyProps == null) return "Invalid body properties";

            if (pawn.BodySize < bodyProps.minBodySize)
                return $"Too small (requires size {bodyProps.minBodySize})";
            if (pawn.BodySize > bodyProps.maxBodySize)
                return $"Too large (maximum size {bodyProps.maxBodySize})";

            if (bodyProps.requiredGender.HasValue && pawn.gender != bodyProps.requiredGender.Value)
                return $"Wrong gender (requires {bodyProps.requiredGender.Value})";

            return null;
        }
    }
}