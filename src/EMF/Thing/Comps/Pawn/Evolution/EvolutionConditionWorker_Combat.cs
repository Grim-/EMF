using RimWorld;
using Verse;

namespace EMF
{
    // Combat experience
    public class CombatEvolutionProperties : EvolutionConditionProperties
    {
        public int minKills = 0;
        public int minMeleeSkill = 0;
        public int minShootingSkill = 0;
    }
    public class EvolutionConditionWorker_Combat : BaseEvolutionConditionWorker
    {
        public override bool MeetsCriteria(Pawn pawn)
        {
            var combatProps = props as CombatEvolutionProperties;
            if (combatProps == null) return true;

            if (pawn.records?.GetAsInt(RecordDefOf.Kills) < combatProps.minKills)
                return false;

            if (pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level < combatProps.minMeleeSkill)
                return false;

            if (pawn.skills?.GetSkill(SkillDefOf.Shooting)?.Level < combatProps.minShootingSkill)
                return false;

            return true;
        }

        public override string GetFailureReason(Pawn pawn)
        {
            var combatProps = props as CombatEvolutionProperties;
            if (combatProps == null) return "Invalid combat properties";

            var kills = pawn.records?.GetAsInt(RecordDefOf.Kills) ?? 0;
            if (kills < combatProps.minKills)
                return $"Not enough kills ({kills}/{combatProps.minKills})";

            var meleeLevel = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0;
            if (meleeLevel < combatProps.minMeleeSkill)
                return $"Melee skill too low ({meleeLevel}/{combatProps.minMeleeSkill})";

            var shootingLevel = pawn.skills?.GetSkill(SkillDefOf.Shooting)?.Level ?? 0;
            if (shootingLevel < combatProps.minShootingSkill)
                return $"Shooting skill too low ({shootingLevel}/{combatProps.minShootingSkill})";

            return null;
        }
    }

}