using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EMF
{
    public class PawnRequirementParms
    {
        public List<RequiredSkillLevel> requiredSkills;
        public List<TraitDef> requiredTraits;
        public List<PawnKindDef> allowedPawnKinds;
        public List<PawnKindDef> disallowedPawnKinds;
        public int minimumAge = -1;
        public List<BackstoryDef> requiredBackstories;
        public ResearchProjectDef requiredResearch;
        public List<HediffDef> requiredHediffs;
        public bool requiresRoyalTitle;
        public RoyalTitleDef minimumTitle;

        public bool MeetsRequirements(Pawn p)
        {
            if (p == null)
                return false;

            if (!CheckPawnKind(p))
                return false;

            if (!CheckSkills(p))
                return false;

            if (!CheckTraits(p))
                return false;

            if (!CheckAge(p))
                return false;

            if (!CheckBackstory(p))
                return false;

            if (!CheckResearch())
                return false;

            if (!CheckHediffs(p))
                return false;

            if (!CheckRoyalty(p))
                return false;

            return true;
        }

        public bool CheckPawnKind(Pawn p)
        {
            if (allowedPawnKinds != null && allowedPawnKinds.Count > 0)
            {
                return allowedPawnKinds.Contains(p.kindDef);
            }

            if (disallowedPawnKinds != null && disallowedPawnKinds.Contains(p.kindDef))
            {
                return false;
            }

            return true;
        }

        public bool CheckSkills(Pawn p)
        {
            if (requiredSkills == null)
                return true;

            return requiredSkills.All(skillReq =>
            {
                var skill = p.skills?.GetSkill(skillReq.skill);
                return skill != null && skill.Level >= skillReq.minLevel;
            });
        }

        public bool CheckTraits(Pawn p)
        {
            if (requiredTraits == null || requiredTraits.Count == 0)
                return true;

            return requiredTraits.All(trait =>
                p.story?.traits.allTraits.Any(t => t.def == trait) ?? false);
        }

        public bool CheckAge(Pawn p)
        {
            if (minimumAge <= 0) return true;
            return p.ageTracker.AgeBiologicalYears >= minimumAge;
        }

        public bool CheckBackstory(Pawn p)
        {
            if (requiredBackstories == null || requiredBackstories.Count == 0)
                return true;

            return requiredBackstories.Any(backStory =>
                p.story?.Childhood == backStory || p.story?.Adulthood == backStory);
        }

        public bool CheckResearch()
        {
            if (requiredResearch == null) return true;
            return requiredResearch.IsFinished;
        }

        public bool CheckHediffs(Pawn p)
        {
            if (requiredHediffs == null || requiredHediffs.Count == 0) return true;

            return requiredHediffs.All(hediff =>
                p.health?.hediffSet.HasHediff(hediff) ?? false);
        }

        public bool CheckRoyalty(Pawn p)
        {
            if (!requiresRoyalTitle && minimumTitle == null) return true;

            var royalTitle = p.royalty?.MostSeniorTitle;
            if (royalTitle == null) return false;

            if (minimumTitle != null)
            {
                return royalTitle.def.seniority >= minimumTitle.seniority;
            }

            return true;
        }


        public string RequirementsExplanation()
        {
            StringBuilder info = new StringBuilder();

            if (allowedPawnKinds?.Count > 0)
            {
                info.AppendLine($"Required type: {string.Join(", ", allowedPawnKinds.Select(pk => pk.label.Trim()))}");
            }

            if (requiredSkills?.Count > 0)
            {
                info.AppendLine($"Required skills: {string.Join(", ", requiredSkills.Select(s => $"{s.skill.label.Trim()} {s.minLevel}"))}");
            }

            if (requiredTraits?.Count > 0)
            {
                var traitLabels = requiredTraits
                    .Select(t => t.label.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l));
                info.AppendLine($"Required traits: {string.Join(", ", traitLabels)}");
            }

            if (minimumTitle != null)
            {
                info.AppendLine($"Minimum title: {minimumTitle.label.Trim()}");
            }

            return info.ToString().TrimEndNewlines();
        }
    }
}
