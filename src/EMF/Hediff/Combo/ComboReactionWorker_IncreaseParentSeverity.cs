using Verse;

namespace EMF
{
    public class ComboReactionWorker_IncreaseParentSeverity : ComboReactionWorker
    {
        public override void DoReaction(Pawn Pawn)
        {
            base.DoReaction(Pawn);

            if (this.Def.reactionProperties != null && this.Def.reactionProperties is AddHediffReactionProperties hediffProperties)
            {
                if (Def.reactionProperties.isAOE)
                {
                    TargetUtil.ApplyHediffSeverityInRadius(hediffProperties.hediff,
                        Pawn.Position,
                        Pawn.Map,
                        Def.reactionProperties.radius,
                        Pawn.Faction,
                        Def.reactionProperties.severityRange.RandomInRange,
                        Def.reactionProperties.friendlyFireSetting,
                        true);
                }
                else
                {
                    Pawn.TakeDamage(new DamageInfo(Def.reactionProperties.damageDef, Def.reactionProperties.damageRange.RandomInRange, Def.reactionProperties.armourpenRange.RandomInRange));
                }
            }

            if (this.Def.reactionProperties != null)
            {
                this.Comp.parent.Severity += this.Def.reactionProperties.severityRange.RandomInRange;
            }
        }

        public override string GetExplanation(Pawn Pawn)
        {
            string explanation = string.Empty;

            if (this.Def.reactionProperties != null && this.Def.reactionProperties is AddHediffReactionProperties hediffProperties)
            {

                string aoe = hediffProperties.isAOE ? $"In a {hediffProperties.radius} radius." : "";
                explanation += $"Increases the severity of {this.Comp.parent.Label} by {Def.reactionProperties.severityRange.min} - {Def.reactionProperties.severityRange.max}  {aoe}";
            }

            return explanation;
        }
    }
}