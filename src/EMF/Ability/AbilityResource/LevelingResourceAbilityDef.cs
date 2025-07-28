using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class LevelingResourceAbilityDef : ResourceAbilityDef
    {
        public List<AbilityLevelData> levels = new List<AbilityLevelData>();
        public float baseExperienceGain = 1f;
        public bool showExperienceInTooltip = true;

        public LevelingResourceAbilityDef()
        {
            abilityClass = typeof(LevelingResourceAbility);
        }

        public AbilityLevelData GetLevelData(int level)
        {
            if (levels.NullOrEmpty() || level < 0)
                return null;

            int clampedLevel = Mathf.Min(level, levels.Count - 1);
            return levels[clampedLevel];
        }

        public int GetMaxLevel() => levels?.Count ?? 1;
    }

    // Data for each ability level
    public class AbilityLevelData
    {
        public string labelSuffix = "";
        public string description = "";
        public float experienceRequired = 100f;

        public float? cooldownMultiplier;
        public float? resourceCostMultiplier;
        public float? rangeMultiplier;
        public float? damageMultiplier;

        public List<AbilityCompProperties> levelComps = new List<AbilityCompProperties>();

        public bool cumulativeEffects = false;

        // Different verb properties per level
        public VerbProperties levelVerbProperties;

        // Visual changes
        public string iconPath;
        public Color? glowColor;
    }

    // The leveling resource ability implementation
    public class LevelingResourceAbility : ResourceAbility, IExposable
    {
        private float experience = 0f;
        private int currentLevel = 0;
        private List<AbilityComp> levelComps;

        public LevelingResourceAbilityDef LevelingResourceDef => (LevelingResourceAbilityDef)def;
        public float Experience => experience;
        public int CurrentLevel => currentLevel;
        public int MaxLevel => LevelingResourceDef.GetMaxLevel();

        public LevelingResourceAbility() : base() { }
        public LevelingResourceAbility(Pawn pawn) : base(pawn) { }
        public LevelingResourceAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public LevelingResourceAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public LevelingResourceAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override void Initialize()
        {
            base.Initialize();
            InitializeLevelComps();
        }

        private void InitializeLevelComps()
        {
            //first level) always uses base ability comps only
            if (currentLevel == 0)
            {
                levelComps = null;
                return;
            }

            // Higher levels use levelComps unless cumulativeEffects is tru
            var levelData = LevelingResourceDef.GetLevelData(currentLevel);
            if (levelData?.levelComps != null && levelData.levelComps.Any())
            {
                levelComps = new List<AbilityComp>();
                if (levelData.cumulativeEffects && base.comps != null)
                {
                    levelComps.AddRange(base.comps);
                }
                foreach (var compProps in levelData.levelComps)
                {
                    try
                    {
                        var comp = (AbilityComp)System.Activator.CreateInstance(compProps.compClass);
                        comp.parent = this;
                        comp.Initialize(compProps);
                        levelComps.Add(comp);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error($"Failed to create level comp: {e}");
                    }
                }
            }
            else
            {
                levelComps = null;
            }
        }

        new public virtual string GizmoExtraLabel
        {
            get
            {
                string chargeString = string.Format("{0} / {1}", this.charges, this.maxCharges);
                string levelString = $"Level {CurrentLevel}";
                if (!this.UsesCharges)
                {
                    return levelString;
                }
                return chargeString + levelString;
            }
        }

        public override string Tooltip
        {
            get
            {
                string baseTooltip = base.Tooltip;
                var levelData = LevelingResourceDef.GetLevelData(currentLevel);
                if (levelData != null && !levelData.labelSuffix.NullOrEmpty())
                {
                    string originalLabel = def.LabelCap;
                    string newLabel = originalLabel + levelData.labelSuffix;
                    if (baseTooltip.StartsWith(originalLabel))
                    {
                        baseTooltip = newLabel + baseTooltip.Substring(originalLabel.Length);
                    }
                }

                if (LevelingResourceDef.showExperienceInTooltip)
                {
                    float nextLevelExp = GetExperienceForNextLevel();

                    baseTooltip += $"\n\nLevel: {currentLevel + 1}/{MaxLevel}";

                    if (currentLevel < MaxLevel - 1)
                    {
                        baseTooltip += $"\nExperience: {experience:F0}/{nextLevelExp:F0}";
                        float percent = (experience / nextLevelExp) * 100f;
                        baseTooltip += $" ({percent:F1}%)";
                    }
                    else
                    {
                        baseTooltip += "\nMax Level Reached";
                    }
                }

                return baseTooltip;
            }
        }

        public override AcceptanceReport CanCast
        {
            get
            {
                if (!base.CanCast)
                    return false;

                var effectiveComps = GetEffectiveComps();
                if (effectiveComps != null)
                {
                    foreach (var comp in effectiveComps)
                    {
                        if (!comp.CanCast)
                            return false;
                    }
                }

                return true;
            }
        }

        private List<AbilityComp> GetEffectiveComps()
        {
            if (currentLevel == 0 || levelComps == null)
            {
                return base.comps;
            }
            else
            {
                return levelComps;
            }
        }

        public override bool CanQueueCast
        {
            get
            {
                if (!base.CanQueueCast)
                    return false;

                return true;
            }
        }

        protected override void ConsumeResource()
        {
            if (ResourceDef.resourceDef != null && resourceGene != null)
            {
                if (resourceGene.Has(ResourceDef.resourceDef, ResourceDef.resourceCost))
                {
                    resourceGene.Consume(ResourceDef.resourceCost);
                }
            }
        }

        public new List<CompAbilityEffect> EffectComps
        {
            get
            {
                var effectiveComps = GetEffectiveComps();
                if (effectiveComps == null)
                    return new List<CompAbilityEffect>();

                return effectiveComps.OfType<CompAbilityEffect>().ToList();
            }
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            this.PreActivate(new LocalTargetInfo?(target));

            if (this.def.hostile && this.pawn.mindState != null)
            {
                this.pawn.mindState.lastCombatantTick = Find.TickManager.TicksGame;
            }

            var effectiveEffectComps = this.EffectComps;
            if (effectiveEffectComps.Any())
            {
                this.affectedTargetsCached.Clear();
                this.affectedTargetsCached.AddRange(this.GetAffectedTargets(target));
                this.ApplyEffects(effectiveEffectComps, this.affectedTargetsCached, dest);
            }

            this.preCastActions.Clear();

            GainExperience(LevelingResourceDef.baseExperienceGain);

            return true;
        }

        public void GainExperience(float amount)
        {
            if (currentLevel >= MaxLevel - 1)
                return;

            experience += amount;

            float requiredForNext = GetExperienceForNextLevel();
            if (experience >= requiredForNext)
            {
                LevelUp();
            }
        }

        private float GetExperienceForNextLevel()
        {
            if (currentLevel >= MaxLevel - 1)
                return float.MaxValue;

            var nextLevelData = LevelingResourceDef.GetLevelData(currentLevel + 1);
            return nextLevelData?.experienceRequired ?? 100f;
        }

        private void LevelUp()
        {
            if (currentLevel >= MaxLevel - 1)
                return;

            currentLevel++;
            var newLevelData = LevelingResourceDef.GetLevelData(currentLevel);

            Messages.Message(
                $"{pawn.Name.ToStringShort}'s {def.LabelCap} reached level {currentLevel + 1}!",
                pawn,
                MessageTypeDefOf.PositiveEvent
            );

            InitializeLevelComps();

            float usedExp = GetExperienceForNextLevel();
            experience = Mathf.Max(0, experience - usedExp);
            if (currentLevel < MaxLevel - 1 && experience >= GetExperienceForNextLevel())
            {
                LevelUp();
            }
        }

        //// Get modified values based on current level
        //public float GetLevelModifiedValue(float baseValue, System.Func<AbilityLevelData, float?> modifier)
        //{
        //    var levelData = LevelingResourceDef.GetLevelData(currentLevel);
        //    if (levelData == null) return baseValue; // No level data, use base value

        //    var multiplier = modifier(levelData);
        //    return multiplier.HasValue ? baseValue * multiplier.Value : baseValue;
        //}

        //public float GetModifiedCooldown(float baseCooldown)
        //{
        //    return GetLevelModifiedValue(baseCooldown, ld => ld?.cooldownMultiplier);
        //}

        //public float GetModifiedResourceCost(float baseCost)
        //{
        //    return GetLevelModifiedValue(baseCost, ld => ld?.resourceCostMultiplier);
        //}

        //public float GetModifiedRange(float baseRange)
        //{
        //    return GetLevelModifiedValue(baseRange, ld => ld?.rangeMultiplier);
        //}

        //public float GetModifiedDamage(float baseDamage)
        //{
        //    return GetLevelModifiedValue(baseDamage, ld => ld?.damageMultiplier);
        //}

        //// Override the damage scaling methods to include level modifications
        //public override float GetDamageScalingMultiplier()
        //{
        //    float baseMultiplier = base.GetDamageScalingMultiplier();
        //    return GetModifiedDamage(baseMultiplier);
        //}

        //public override DamageInfo GetModifiedDamage(DamageInfo damageInfo)
        //{
        //    // First apply the resource-based scaling
        //    DamageInfo resourceModified = base.GetModifiedDamage(damageInfo);

        //    // Then apply level-based modifications
        //    float levelDamageMultiplier = GetLevelModifiedValue(1f, ld => ld?.damageMultiplier);
        //    if (levelDamageMultiplier != 1f)
        //    {
        //        resourceModified.SetAmount(resourceModified.Amount * levelDamageMultiplier);
        //        Log.Message($"Level-scaled Damage to {resourceModified.Amount} from {damageInfo.Amount} (Level {currentLevel + 1})");
        //    }

        //    return resourceModified;
        //}

        //public override float GetModifiedDamageAmount(float baseDamage)
        //{
        //    // Apply resource scaling first, then level scaling
        //    float resourceScaled = base.GetModifiedDamageAmount(baseDamage);
        //    return GetModifiedDamage(resourceScaled);
        //}


        public override IEnumerable<Command> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }

            if (DebugSettings.godMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = $"Level up {this.def.LabelCap}",
                    defaultDesc = "level up",
                    action = () =>
                    {
                        LevelUp();
                    }
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref experience, "experience", 0f);
            Scribe_Values.Look(ref currentLevel, "currentLevel", 0);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                // Initialize level comps BEFORE their ExposeData methods are called
                if (this.Id == -1)
                {
                    this.Id = Find.UniqueIDsManager.GetNextAbilityID();
                }
                InitializeLevelComps();
            }

            // Save/load level comp data
            if (levelComps != null)
            {
                for (int i = 0; i < levelComps.Count; i++)
                {
                    levelComps[i].PostExposeData();
                }
            }
        }
    }
}
