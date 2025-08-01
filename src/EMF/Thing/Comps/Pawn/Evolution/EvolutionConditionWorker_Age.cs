using Verse;

namespace EMF
{
    public class AgeEvolutionProperties : EvolutionConditionProperties
    {
        public float minAge = 0f;
        public float maxAge = float.MaxValue;
    }



    public class EvolutionConditionWorker_Age : BaseEvolutionConditionWorker
    {
        public override bool MeetsCriteria(Pawn pawn)
        {
            var ageProps = props as AgeEvolutionProperties;
            if (ageProps == null) return true;

            var age = pawn.ageTracker.AgeBiologicalYears;
            return age >= ageProps.minAge && age <= ageProps.maxAge;
        }

        public override string GetFailureReason(Pawn pawn)
        {
            var ageProps = props as AgeEvolutionProperties;
            if (ageProps == null) return "Invalid age properties";

            var age = pawn.ageTracker.AgeBiologicalYears;
            if (age < ageProps.minAge)
                return $"Too young (requires {ageProps.minAge} years)";
            if (age > ageProps.maxAge)
                return $"Too old (maximum {ageProps.maxAge} years)";

            return null;
        }
    }


}