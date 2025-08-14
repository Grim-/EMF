using RimWorld;
using Verse;
using Verse.AI;

namespace EMF
{
    public class JobGiver_SummonedCreatureFightEnemy : JobGiver_AIDefendPawn
    {
        protected Pawn Master = null;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Error("TryGiveJob called with null pawn");
                return null;
            }

            this.chaseTarget = true;
            this.allowTurrets = true;
            this.ignoreNonCombatants = true;
            this.humanlikesOnly = false;

            Job job = base.TryGiveJob(pawn);

            if (job != null)
            {
                job.reportStringOverride = "Defending Summoner";
            }

            if (pawn.mindState != null)
            {
                pawn.mindState.canFleeIndividual = false;
            }

            return job;
        }

        protected override Pawn GetDefendee(Pawn pawn)
        {
            if (pawn.IsASummon(out HediffComp_Summon summonComp))
            {
                return summonComp.Summoner;
            }
            return null;
        }

        protected override float GetFlagRadius(Pawn pawn)
        {
            return 5f;
        }

        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            if (pawn.IsASummon(out HediffComp_Summon summonComp))
            {
                return summonComp.Summoner.Position.RandomAdjacentCell8Way();
            }

            return base.GetFlagPosition(pawn);
        }
    }
}