using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{

    public class HediffCompProperties_ComboReactor : HediffCompProperties
    { 
        public List<ComboReaction> damageReactions;
        public List<ComboReaction> hediffReactions;


        public HediffCompProperties_ComboReactor()
        {
            compClass = typeof(HediffComp_ComboReactor);
        }

        public bool HasReaction(DamageDef damageDef)
        {
            return damageReactions != null && damageReactions.Any(x=> x.reactionDamageType.Contains(damageDef));
        }



        public List<ComboReaction> GetDamageReactions(DamageDef damageDef)
        {
            if (HasReaction(damageDef))
            {
                return damageReactions.Where(x => x.reactionDamageType.Contains(damageDef)).ToList();
            }

            return null;
        }

        public List<ComboReaction> GetHediffReactions(HediffDef damageDef)
        {
            if (HasReaction(damageDef))
            {
                return hediffReactions.Where(x => x.reactionHediff.Contains(damageDef)).ToList();
            }
            return null;
        }

        public bool HasReaction(HediffDef damageDef)
        {
            return hediffReactions != null && hediffReactions.Any(x => x.reactionHediff.Contains(damageDef));
        }
    }

    public class HediffComp_ComboReactor : HediffComp
    {
        public HediffCompProperties_ComboReactor Props => (HediffCompProperties_ComboReactor)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            //EventManager.OnPawnHediffGained += EventManager_OnPawnHediffGained;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            //EventManager.OnPawnHediffGained -= EventManager_OnPawnHediffGained;
        }

        //damage based reaction
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if (Props.damageReactions != null && dinfo.Def != null)
            {
                if (Props.HasReaction(dinfo.Def))
                {
                    List<ComboReaction> comboReactionDef = Props.GetDamageReactions(dinfo.Def);

                    bool shouldRemove = false;

                    foreach (var item in comboReactionDef)
                    {
                        item.ExecuteWorker(this.Pawn, this);

                        if (item.removeOnReact)
                        {
                            shouldRemove = true;
                        }
                    }

                    if (shouldRemove)
                    {
                        Pawn.health.RemoveHediff(this.parent);
                    }

                }
            }
        }


        public override string CompDescriptionExtra
        {
            get
            {
                string reactionExplanations = base.CompDescriptionExtra;

                if (Props.damageReactions != null && Props.damageReactions.Count > 0)
                {
                    reactionExplanations += "\r\n\r\nDamage Reactions:";
                    foreach (var item in Props.damageReactions)
                    {
                        reactionExplanations += $"\r\n- {string.Join(", ", item.reactionDamageType.ConvertAll(d => d.LabelCap))}: ";
                        reactionExplanations += item.Worker.GetExplanation(this.Pawn);
                        if (item.removeOnReact)
                        {
                            reactionExplanations += " (Consumes effect)";
                        }
                    }
                }

                return reactionExplanations;
            }
        }
    }
}