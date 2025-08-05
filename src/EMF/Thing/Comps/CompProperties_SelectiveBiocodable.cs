using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EMF
{
    public class CompProperties_SelectiveBiocodable : CompProperties_Biocodable
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

        public EffecterDef onBondEffect;

        public CompProperties_SelectiveBiocodable()
        {
            compClass = typeof(CompSelectiveBiocodable);
        }
    }

    public class RequiredSkillLevel
    {
        public SkillDef skill;
        public int minLevel = 1;
    }

    public class CompSelectiveBiocodable : CompBiocodable
    {
        public CompProperties_SelectiveBiocodable SelectiveProps => (CompProperties_SelectiveBiocodable)props;

        public override bool Biocodable => !base.Biocoded;

        public override void CodeFor(Pawn p)
        {
            if (!CanBeBiocodedFor(p))
            {
                Messages.Message($"{p.Name.ToStringShort} cannot biocode {parent.Label}: requirements not met", MessageTypeDefOf.RejectInput, false);
                return;
            }

            base.CodeFor(p);
        }

        public bool CanBeBiocodedFor(Pawn p)
        {
            if (p == null) return false;

            if (!CheckPawnKind(p)) return false;
            if (!CheckSkills(p)) return false;
            if (!CheckTraits(p)) return false;
            if (!CheckAge(p)) return false;
            if (!CheckBackstory(p)) return false;
            if (!CheckResearch()) return false;
            if (!CheckHediffs(p)) return false;
            if (!CheckRoyalty(p)) return false;

            return true;
        }

        private bool CheckPawnKind(Pawn p)
        {
            if (SelectiveProps.allowedPawnKinds != null && SelectiveProps.allowedPawnKinds.Count > 0)
            {
                return SelectiveProps.allowedPawnKinds.Contains(p.kindDef);
            }

            if (SelectiveProps.disallowedPawnKinds != null && SelectiveProps.disallowedPawnKinds.Contains(p.kindDef))
            {
                return false;
            }

            return true;
        }

        private bool CheckSkills(Pawn p)
        {
            if (SelectiveProps.requiredSkills == null) return true;

            return SelectiveProps.requiredSkills.All(skillReq =>
            {
                var skill = p.skills?.GetSkill(skillReq.skill);
                return skill != null && skill.Level >= skillReq.minLevel;
            });
        }

        private bool CheckTraits(Pawn p)
        {
            if (SelectiveProps.requiredTraits == null || SelectiveProps.requiredTraits.Count == 0) return true;

            return SelectiveProps.requiredTraits.All(trait =>
                p.story?.traits.allTraits.Any(t => t.def == trait) ?? false);
        }

        private bool CheckAge(Pawn p)
        {
            if (SelectiveProps.minimumAge <= 0) return true;
            return p.ageTracker.AgeBiologicalYears >= SelectiveProps.minimumAge;
        }

        private bool CheckBackstory(Pawn p)
        {
            if (SelectiveProps.requiredBackstories == null || SelectiveProps.requiredBackstories.Count == 0) return true;

            return SelectiveProps.requiredBackstories.Any(backStory =>
                p.story?.Childhood == backStory || p.story?.Adulthood == backStory);
        }

        private bool CheckResearch()
        {
            if (SelectiveProps.requiredResearch == null) return true;
            return SelectiveProps.requiredResearch.IsFinished;
        }

        private bool CheckHediffs(Pawn p)
        {
            if (SelectiveProps.requiredHediffs == null || SelectiveProps.requiredHediffs.Count == 0) return true;

            return SelectiveProps.requiredHediffs.All(hediff =>
                p.health?.hediffSet.HasHediff(hediff) ?? false);
        }

        private bool CheckRoyalty(Pawn p)
        {
            if (!SelectiveProps.requiresRoyalTitle && SelectiveProps.minimumTitle == null) return true;

            var royalTitle = p.royalty?.MostSeniorTitle;
            if (royalTitle == null) return false;

            if (SelectiveProps.minimumTitle != null)
            {
                return royalTitle.def.seniority >= SelectiveProps.minimumTitle.seniority;
            }

            return true;
        }

        public override string CompInspectStringExtra()
        {
            var info = new StringBuilder(base.CompInspectStringExtra());

            if (Biocoded) return info.ToString();

            AppendRequirements(info);
            return info.ToString().TrimEndNewlines();
        }

        private void AppendRequirements(StringBuilder info)
        {
            if (SelectiveProps.allowedPawnKinds?.Count > 0)
            {
                info.AppendLine($"Required type: {string.Join(", ", SelectiveProps.allowedPawnKinds.Select(pk => pk.label.Trim()))}");
            }

            if (SelectiveProps.requiredSkills?.Count > 0)
            {
                info.AppendLine($"Required skills: {string.Join(", ", SelectiveProps.requiredSkills.Select(s => $"{s.skill.label.Trim()} {s.minLevel}"))}");
            }

            if (SelectiveProps.requiredTraits?.Count > 0)
            {
                var traitLabels = SelectiveProps.requiredTraits
                    .Select(t => t.label.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l));
                info.AppendLine($"Required traits: {string.Join(", ", traitLabels)}");
            }

            if (SelectiveProps.minimumTitle != null)
            {
                info.AppendLine($"Minimum title: {SelectiveProps.minimumTitle.label.Trim()}");
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var entry in base.SpecialDisplayStats())
            {
                yield return entry;
            }

            if (!Biocoded)
            {
                if (SelectiveProps.minimumAge > 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn,
                        "Minimum age", SelectiveProps.minimumAge.ToString(),
                        "Minimum biological age required to biocode this item.", 1000);
                }

                if (SelectiveProps.requiredResearch != null)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn,
                        "Required research", SelectiveProps.requiredResearch.label,
                        "Research project required to biocode this item.", 1001);
                }
            }
        }
    }
}
