using Verse;
using Verse.AI;

namespace EMF
{
    public class ThinkNodeConditional_IsSummonNearSummoner : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.IsASummon(out HediffComp_Summon summonComp))
            {
                if (summonComp.Summoner != null && pawn.Position.DistanceTo(summonComp.Summoner.Position) <= summonComp.Props.followDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}