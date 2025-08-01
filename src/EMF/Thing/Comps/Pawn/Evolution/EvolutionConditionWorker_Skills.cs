using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class SkillEvolutionProperties : EvolutionConditionProperties
    {
        public List<SkillRequirement> skillRequirements = new List<SkillRequirement>();
    }


    public class SkillRequirement
    {
        public SkillDef skill;
        public int minLevel;
    }

    public class EvolutionConditionWorker_Skills : BaseEvolutionConditionWorker
    {
        public override bool MeetsCriteria(Pawn pawn)
        {
            var skillProps = props as SkillEvolutionProperties;
            if (skillProps == null || skillProps.skillRequirements == null) return true;

            foreach (var req in skillProps.skillRequirements)
            {
                if (pawn.skills?.GetSkill(req.skill)?.Level < req.minLevel)
                    return false;
            }
            return true;
        }

        public override string GetFailureReason(Pawn pawn)
        {
            var skillProps = props as SkillEvolutionProperties;
            if (skillProps == null) return "Invalid skill properties";

            foreach (var req in skillProps.skillRequirements)
            {
                var level = pawn.skills?.GetSkill(req.skill)?.Level ?? 0;
                if (level < req.minLevel)
                    return $"{req.skill.label} too low ({level}/{req.minLevel})";
            }
            return null;
        }
    }
}