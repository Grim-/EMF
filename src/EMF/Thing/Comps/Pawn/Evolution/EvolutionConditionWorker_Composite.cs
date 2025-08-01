using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompositeEvolutionProperties : EvolutionConditionProperties
    {
        public List<EvolutionCondition> conditions = new List<EvolutionCondition>();
        public bool requireAll = true;
    }

    public class EvolutionConditionWorker_Composite : BaseEvolutionConditionWorker
    {
        public override bool MeetsCriteria(Pawn pawn)
        {
            var compositeProps = props as CompositeEvolutionProperties;
            if (compositeProps == null || compositeProps.conditions == null) return true;

            if (compositeProps.requireAll)
            {
                foreach (var condition in compositeProps.conditions)
                {
                    var worker = condition.CreateWorker();
                    if (!worker.MeetsCriteria(pawn))
                        return false;
                }
                return true;
            }
            else
            {
                foreach (var condition in compositeProps.conditions)
                {
                    var worker = condition.CreateWorker();
                    if (worker.MeetsCriteria(pawn))
                        return true;
                }
                return false;
            }
        }

        public override string GetFailureReason(Pawn pawn)
        {
            var compositeProps = props as CompositeEvolutionProperties;
            if (compositeProps == null) return "Invalid composite properties";

            var failureReasons = new List<string>();

            foreach (var condition in compositeProps.conditions)
            {
                var worker = condition.CreateWorker();
                if (!worker.MeetsCriteria(pawn))
                {
                    var reason = worker.GetFailureReason(pawn);
                    if (!string.IsNullOrEmpty(reason))
                        failureReasons.Add(reason);
                }
            }

            if (compositeProps.requireAll && failureReasons.Count > 0)
                return string.Join(", ", failureReasons);
            else if (!compositeProps.requireAll && failureReasons.Count == compositeProps.conditions.Count)
                return "No conditions met: " + string.Join(" OR ", failureReasons);

            return null;
        }
    }
}