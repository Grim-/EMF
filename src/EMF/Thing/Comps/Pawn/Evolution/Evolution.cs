using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class Evolution
    {
        public List<EvolutionCondition> conditions = new List<EvolutionCondition>();
        public string label;
        public string description;


        public List<PawnKindDef> GetAllAvailableEvolutions(Pawn pawn)
        {
            List<PawnKindDef> available = new List<PawnKindDef>();
            foreach (var condition in conditions)
            {
                var worker = condition.CreateWorker();
                if (worker.MeetsCriteria(pawn) || Prefs.DevMode)
                {
                    available.Add(condition.newKind);
                }
            }
            return available;
        }
    }
}