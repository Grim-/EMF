using Verse;

namespace EMF
{
    public class StunReactionProperties : ComboReactionProperties
    {
        public IntRange stunTicks = new IntRange(300, 300);
    }

    public class ComboReactionWorker_Stun : ComboReactionWorker
    {
        public override void DoReaction(Pawn Pawn)
        {
            base.DoReaction(Pawn);

            if (this.Def.reactionProperties != null && this.Def.reactionProperties is StunReactionProperties reactionProperties)
            {
                if (Pawn.stances?.stunner != null)
                {
                    Pawn.stances.stunner.StunFor(reactionProperties.stunTicks.RandomInRange, Pawn);
                }
            }
        }

        public override string GetExplanation(Pawn Pawn)
        {
            string explanation = string.Empty;

            if (this.Def.reactionProperties != null && this.Def.reactionProperties is StunReactionProperties hediffProperties)
            {

                string aoe = hediffProperties.isAOE ? $"In a {hediffProperties.radius} radius." : "";
                explanation += $"Stuns for {hediffProperties.stunTicks.RandomInRange.ToStringSecondsFromTicks()} {aoe}";
            }

            return explanation;
        }
    }
}