using Verse;
using Verse.AI;

namespace EMF
{
    public class ThinkNodeConditional_IsSummon : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.IsASummon(out HediffComp_Summon summonComp))
            {
                if (summonComp.Summoner != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}