using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class Gene_BasicResource : Gene_Resource, IGeneResourceDrain
    {
        public BasicResourceGeneDef Def => def != null ? (BasicResourceGeneDef)def : null;
        public AbilityResourceDef ResourceDef => Def?.primaryResourceDef;

        public bool EnableResource = true;
        public Gene_Resource Resource => this;
        public Pawn Pawn => pawn;

        private Dictionary<AbilityResourceDef, ResourceData> additionalResources = new Dictionary<AbilityResourceDef, ResourceData>();
        public Dictionary<AbilityResourceDef, ResourceData> AdditionalResources => additionalResources;

        private Dictionary<AbilityResourceDef, GeneGizmo_BasicResource> gizmos = new Dictionary<AbilityResourceDef, GeneGizmo_BasicResource>();
        private Dictionary<AbilityResourceDef, bool> additionalResourceVisibility = new Dictionary<AbilityResourceDef, bool>();

        private List<AbilityResourceDef> resourcesWorkingListKeys = new List<AbilityResourceDef>();
        private List<ResourceData> resourcesWorkingListValues = new List<ResourceData>();

        public bool CanOffset
        {
            get
            {
                if (IsLocked)
                {
                    return false;
                }

                if (Active)
                {
                    return !pawn.Deathresting;
                }
                return false;
            }
        }

        private bool _IsLocked = false;
        public bool IsLocked
        {
            get
            {
                return _IsLocked;
            }
            set => _IsLocked = value;
        }

        private bool _IsRegenEnabled = true;
        public bool IsRegenEnabled
        {
            get
            {
                return _IsRegenEnabled;
            }
            set => _IsRegenEnabled = value;
        }

        private int CurrentRegenTick = 0;

        public override float Value
        {
            get => base.Value;
            set => base.Value = Mathf.Clamp(value, 0f, Max);
        }

        public float ValueCostMultiplied => Value * CostMult;
        public string DisplayLabel => ResourceDef?.resourceName ?? "Unknown Resource";
        public float ResourceLossPerDay => def?.resourceLossPerDay ?? 0f;
        public override float InitialResourceMax => ResourceDef?.MaxStatValue(Pawn) ?? 100f;
        public override float MinLevelForAlert => 0.15f;
        public override float MaxLevelOffset => 0.1f;

        protected float defaultMax = 10f;

        private float lastMax;
        public override float Max
        {
            get
            {
                if (ResourceDef == null)
                    return defaultMax;

                float currentMax = ResourceDef.MaxStatValue(Pawn);
                if (currentMax != lastMax)
                {
                    lastMax = currentMax;
                    ForceBaseMaxUpdate(currentMax);
                }
                return currentMax;
            }
        }

        protected override Color BarColor => ResourceDef?.barColor ?? new ColorInt(3, 3, 138).ToColor;
        protected override Color BarHighlightColor => new ColorInt(42, 42, 145).ToColor;

        public override int ValueForDisplay => Mathf.RoundToInt(Value);
        public override int MaxForDisplay => Mathf.RoundToInt(Max);

        public float RegenAmount => ResourceDef?.RegenStatValue(Pawn) ?? 1f;
        public float RegenSpeed => ResourceDef?.RegenSpeedStatValue(Pawn) ?? 1f;
        public int RegenTicks => ResourceDef != null ? Mathf.RoundToInt(ResourceDef.RegenTicksValue(Pawn) / RegenSpeed) : Mathf.RoundToInt(2500 / RegenSpeed);
        public float CostMult => ResourceDef?.CostMultValue(Pawn) ?? 1f;

        public float TotalResourceUsed = 0;

        public override void PostAdd()
        {
            if (ModLister.CheckBiotech("Hemogen"))
            {
                base.PostAdd();
                Reset();
                InitializeAdditionalResources();
            }
        }

        private void ForceBaseMaxUpdate(float newMax)
        {
            this.SetMax(newMax);
        }

        private void InitializeAdditionalResources()
        {
            additionalResources.Clear();
            gizmos.Clear();

            if (additionalResourceVisibility == null)
                additionalResourceVisibility = new Dictionary<AbilityResourceDef, bool>();

            if (Def?.additionalResources != null)
            {
                foreach (var extraResource in Def.additionalResources)
                {
                    if (extraResource.resourceDef != null)
                    {
                        TryGainAdditionalResource(extraResource.resourceDef);
                        if (!additionalResourceVisibility.ContainsKey(extraResource.resourceDef))
                        {
                            additionalResourceVisibility[extraResource.resourceDef] = true;
                        }
                    }
                }
            }
        }

        public void Consume(float Amount, bool addToUsedTotal = true)
        {
            Consume(ResourceDef, Amount, addToUsedTotal);
        }

        public void Consume(AbilityResourceDef resourceDef, float Amount, bool addToUsedTotal = true)
        {
            if (!ModsConfig.BiotechActive)
                return;

            if (resourceDef == ResourceDef)
            {
                if (IsLocked)
                    return;

                if (addToUsedTotal) TotalResourceUsed += Amount;
                Value -= Amount * CostMult;
            }
            else
            {
                ResourceData resourceData = GetAdditionalResource(resourceDef);
                if (resourceData == null || resourceData.isLocked)
                    return;

                float costMult = resourceData.resourceDef?.CostMultValue(Pawn) ?? 1f;

                if (addToUsedTotal)
                    resourceData.totalResourceUsed += Amount;

                resourceData.currentValue = Mathf.Max(0f, resourceData.currentValue - (Amount * costMult));
            }
        }

        public void Restore(float Amount)
        {
            Restore(ResourceDef, Amount);
        }

        public void Restore(AbilityResourceDef resourceDef, float Amount)
        {
            if (!ModsConfig.BiotechActive)
                return;

            if (resourceDef == ResourceDef)
            {
                if (IsLocked)
                    return;

                Value += Amount;
            }
            else
            {
                ResourceData resourceData = GetAdditionalResource(resourceDef);
                if (resourceData != null)
                {
                    resourceData.currentValue = Mathf.Min(resourceData.maxValue, resourceData.currentValue + Amount);
                }
            }
        }

        public bool Has(float Amount)
        {
            return Has(ResourceDef, Amount);
        }



        public bool HasResource(AbilityResourceDef resourceDef)
        {
            if (ResourceDef == resourceDef || additionalResources.ContainsKey(resourceDef))
            {
                return true;
            }

            return false;
        }

        public bool Has(AbilityResourceDef resourceDef, float Amount)
        {
            if (!ModsConfig.BiotechActive)
                return false;

            if (resourceDef == ResourceDef)
            {
                if (ResourceIsUnavailable(out string reason))
                    return false;

                return Value >= Amount * CostMult;
            }
            else
            {
                ResourceData resourceData = GetAdditionalResource(resourceDef);

                if (resourceData == null || resourceData.isLocked || !resourceData.enableResource)
                    return false;

                float costMult = resourceData.resourceDef?.CostMultValue(Pawn) ?? 1f;
                return resourceData.currentValue >= (Amount * costMult);
            }
        }

        public bool TryGainAdditionalResource(AbilityResourceDef newResourceDef)
        {
            if (newResourceDef != null && newResourceDef != ResourceDef)
            {
                if (additionalResources.ContainsKey(newResourceDef))
                {
                    return false;
                }

                float initialMax = newResourceDef.MaxStatValue(Pawn);
                ResourceData newData = new ResourceData(newResourceDef, initialMax);
                additionalResources.Add(newResourceDef, newData);
                TryInitGizmoForResource(newResourceDef, newData);
                return true;
            }

            return false;
        }

        public bool TryRemoveAdditionalResource(AbilityResourceDef resourceDef)
        {
            if (resourceDef != null && resourceDef != ResourceDef && additionalResources.ContainsKey(resourceDef))
            {
                additionalResources.Remove(resourceDef);
                if (gizmos.ContainsKey(resourceDef))
                    gizmos.Remove(resourceDef);
                return true;
            }

            return false;
        }

        private void TryInitGizmoForResource(AbilityResourceDef pawnResourceDef, ResourceData resourceData)
        {
            if (!gizmos.ContainsKey(pawnResourceDef))
            {
                gizmos.Add(pawnResourceDef, new GeneGizmo_BasicResource(this, resourceData, null, pawnResourceDef.barColor, pawnResourceDef.barHighlightColor));
            }
        }

        public bool HasAdditionalResource(AbilityResourceDef pawnResourceDef)
        {
            return additionalResources.ContainsKey(pawnResourceDef);
        }

        private Gizmo GetGizmoForResource(AbilityResourceDef pawnResourceDef)
        {
            return gizmos[pawnResourceDef];
        }

        public ResourceData GetAdditionalResource(AbilityResourceDef resourceDef)
        {
            return additionalResources.ContainsKey(resourceDef) ? additionalResources[resourceDef] : null;
        }

        public override void Tick()
        {
            base.Tick();

            if (IsRegenEnabled)
            {
                CurrentRegenTick++;
                if (CurrentRegenTick >= RegenTicks)
                {
                    Restore(RegenAmount);
                    ResetRegenTicks();
                }
            }

            TickAdditionalResources();
        }

        protected void TickAdditionalResources()
        {
            foreach (var resource in additionalResources.Values)
            {
                TickResource(resource);
            }
        }

        private void TickResource(ResourceData resource)
        {
            if (resource.isRegenEnabled && resource.resourceDef != null)
            {
                resource.currentRegenTick++;

                float regenAmount = resource.resourceDef.RegenStatValue(Pawn);
                float regenSpeed = resource.resourceDef.RegenSpeedStatValue(Pawn);
                int regenTicks = Mathf.RoundToInt(resource.resourceDef.RegenTicksValue(Pawn) / regenSpeed);

                if (resource.currentRegenTick >= regenTicks)
                {
                    Restore(resource.resourceDef, regenAmount);
                    resource.currentRegenTick = 0;
                }
            }

            float newMax = resource.resourceDef?.MaxStatValue(Pawn) ?? resource.maxValue;
            if (newMax != resource.maxValue)
            {
                resource.maxValue = newMax;
                resource.currentValue = Mathf.Clamp(resource.currentValue, 0f, resource.maxValue);
                resource.targetValue = Mathf.Clamp(resource.targetValue, 0f, resource.maxValue - MaxLevelOffset);
            }
        }

        public void ResetRegenTicks()
        {
            CurrentRegenTick = 0;
        }

        public override void SetTargetValuePct(float val)
        {
            targetValue = Mathf.Clamp(val * Max, 0f, Max - MaxLevelOffset);
        }

        public bool ResourceIsUnavailable(out string reason)
        {
            if (IsLocked)
            {
                reason = "Resource Locked";
                return true;
            }

            if (!EnableResource)
            {
                reason = "Resource not enabled";
                return true;
            }

            reason = string.Empty;
            return false;
        }

        public bool IsAdditionalResourceVisible(AbilityResourceDef resourceDef)
        {
            if (!additionalResourceVisibility.ContainsKey(resourceDef))
            {
                additionalResourceVisibility[resourceDef] = true;
            }
            return additionalResourceVisibility[resourceDef];
        }

        public void ToggleAdditionalResourceVisibility(AbilityResourceDef resourceDef)
        {
            if (!additionalResourceVisibility.ContainsKey(resourceDef))
            {
                additionalResourceVisibility[resourceDef] = true;
            }
            additionalResourceVisibility[resourceDef] = !additionalResourceVisibility[resourceDef];
        }

        public void SetAdditionalResourceVisibility(AbilityResourceDef resourceDef, bool visible)
        {
            additionalResourceVisibility[resourceDef] = visible;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (DebugSettings.godMode)
            {
                float halfMaxPrimary = this.Max / 2;

                yield return new Command_Action()
                {
                    defaultLabel = $"Add {halfMaxPrimary} {ResourceLabel}",
                    defaultDesc = $"Add {halfMaxPrimary} {ResourceLabel}",
                    icon = TexButton.Add,
                    action = () =>
                    {
                        this.Restore(halfMaxPrimary);
                    }
                };

                yield return new Command_Action()
                {
                    defaultLabel = $"Remove {halfMaxPrimary} {ResourceLabel}",
                    defaultDesc = $"Remove {halfMaxPrimary} {ResourceLabel}",
                    icon = TexButton.Delete,
                    action = () =>
                    {
                        this.Consume(halfMaxPrimary, false);
                    }
                };

                foreach (var resource in additionalResources.Values)
                {
                    if (resource?.resourceDef == null)
                        continue;

                    float halfMax = resource.maxValue / 2;

                    yield return new Command_Action()
                    {
                        defaultLabel = $"Add {halfMax} {resource.resourceDef.resourceName}",
                        defaultDesc = $"Add {halfMax} {resource.resourceDef.resourceName}",
                        icon = TexButton.Add,
                        action = () => Restore(resource.resourceDef, halfMax)
                    };

                    yield return new Command_Action()
                    {
                        defaultLabel = $"Remove {halfMax} {resource.resourceDef.resourceName}",
                        defaultDesc = $"Remove {halfMax} {resource.resourceDef.resourceName}",
                        icon = TexButton.Delete,
                        action = () => Consume(resource.resourceDef, halfMax, false)
                    };
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableResource, "resourceEnabled", defaultValue: true);
            Scribe_Values.Look(ref CurrentRegenTick, "currentRegenTick", defaultValue: 0);
            Scribe_Values.Look(ref TotalResourceUsed, "TotalResourceUsed", defaultValue: 0);
            Scribe_Collections.Look<AbilityResourceDef, ResourceData>(ref additionalResources, "additionalResources", LookMode.Def, LookMode.Deep, ref resourcesWorkingListKeys, ref resourcesWorkingListValues);
            Scribe_Collections.Look<AbilityResourceDef, bool>(ref additionalResourceVisibility, "additionalResourceVisibility", LookMode.Def, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (additionalResources == null)
                {
                    additionalResources = new Dictionary<AbilityResourceDef, ResourceData>();
                }
                if (additionalResourceVisibility == null)
                {
                    additionalResourceVisibility = new Dictionary<AbilityResourceDef, bool>();
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            InitializeAdditionalResources();
        }
    }
}
