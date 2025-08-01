using System;
using Verse;

namespace EMF
{
    public class EvolutionConditionProperties
    {

    }
    public class EvolutionCondition
    {
        public PawnKindDef newKind;
        public EvolutionConditionProperties props;
        public Type conditionWorker = typeof(BaseEvolutionConditionWorker);

        public BaseEvolutionConditionWorker CreateWorker()
        {
            BaseEvolutionConditionWorker condition = (BaseEvolutionConditionWorker)Activator.CreateInstance(conditionWorker);
            condition.props = props;
            condition.newKind = newKind;
            return condition;
        }
    }
}