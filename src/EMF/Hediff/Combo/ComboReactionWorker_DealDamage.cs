using Verse;

namespace EMF
{
    public class ComboReactionWorker_DealDamage : ComboReactionWorker
    {
        public override void DoReaction(Pawn Pawn)
        {
            base.DoReaction(Pawn);

            if (this.Def.reactionProperties != null)
            {
                if (Def.reactionProperties.isAOE)
                {
                    TargetUtil.ApplyDamageInRadius(Def.reactionProperties.damageDef, 
                        Def.reactionProperties.damageRange.RandomInRange, 
                        Def.reactionProperties.armourpenRange.RandomInRange,
                        Pawn.Position, 
                        Pawn.Map, 
                        Def.reactionProperties.radius,
                        Pawn.Faction,
                        Def.reactionProperties.friendlyFireSetting,
                        true, 
                        Pawn);
                }
                else
                {
                    Pawn.TakeDamage(new DamageInfo(Def.reactionProperties.damageDef, Def.reactionProperties.damageRange.RandomInRange, Def.reactionProperties.armourpenRange.RandomInRange));
                }           
            }
        }

        public override string GetExplanation(Pawn Pawn)
        {
            string explanation = string.Empty;

            if (this.Def.reactionProperties != null)
            {
                string aoe = Def.reactionProperties.isAOE ? $"In a {Def.reactionProperties.radius} radius." : "";
                explanation += $"Deals {Def.reactionProperties.damageRange.min} -{Def.reactionProperties.damageRange.max} {Def.reactionProperties.damageDef.LabelCap} damage {aoe}";
            }

            return explanation;
        }
    }
}