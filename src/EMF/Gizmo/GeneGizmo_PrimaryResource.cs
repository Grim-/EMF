using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    [StaticConstructorOnStartup]
    public class GeneGizmo_PrimaryResource : GeneGizmo_BasicResource
    {
        private const float TotalPulsateTime = 0.85f;
        private const float IconSize = 16f;
        private const float IconSpacing = 18f;
        private const float IconMargin = 4f;

        private List<Pair<IGeneResourceDrain, float>> tmpDrainGenes = new List<Pair<IGeneResourceDrain, float>>();
        private Gene_BasicResource basicResourceGene;

        protected override string Title
        {
            get
            {
                if (basicResourceGene?.ResourceDef != null)
                {
                    return basicResourceGene?.ResourceDef.label;
                }
                return base.Title;
            }
        }
        protected override float ValuePercent
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return basicResource.ValuePercent;
                }

                return base.ValuePercent;
            }
        }

        protected override string BarLabel
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return $"{basicResource.ValueForDisplay} / {basicResource.MaxForDisplay}";
                }

                return base.BarLabel;
            }
        }

        protected override Color BarColor
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return basicResource.ResourceDef.barColor;
                }

                return base.BarColor;
            }
        }

        protected override Color BarHighlightColor
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return basicResource.ResourceDef.barHighlightColor;
                }

                return base.BarHighlightColor;
            }
        }
        public GeneGizmo_PrimaryResource(Gene_BasicResource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor) : base(gene, drainGenes, barColor, barHighlightColor)
        {
            if (gene == null)
            {
                Log.Error("GeneGizmo_PrimaryResourceWithToggles created with null gene");
                return;
            }
            basicResourceGene = gene;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            var additionalResources = GetAdditionalResources();
            if (additionalResources.Any())
            {
                DrawAdditionalResourceIcons(topLeft, maxWidth, additionalResources);
            }

            DrawMaintenanceCostOverlay(topLeft, maxWidth);

            return result;
        }

        private void DrawMaintenanceCostOverlay(Vector2 topLeft, float maxWidth)
        {
            var maintenanceCosts = GetMaintenanceCosts();
            if (!maintenanceCosts.Any() || basicResourceGene.Max <= 0)
                return;

            float gizmoWidth = GetWidth(maxWidth);
            float barHeight = 14f;
            float barY = topLeft.y + 45f;
            float barX = topLeft.x + 8f;
            float barWidth = gizmoWidth - 16f;

            float totalMaintenance = maintenanceCosts.Sum(x => x.cost);
            float maintenancePercent = Mathf.Clamp01(totalMaintenance / basicResourceGene.Max);

            if (maintenancePercent > 0)
            {
                float overlayWidth = barWidth * maintenancePercent;
                Rect overlayRect = new Rect(barX, barY, overlayWidth, barHeight);

                Color overlayColor = new Color(1f, 0.3f, 0.3f, 0.6f);
                GUI.color = overlayColor;
                GUI.DrawTexture(overlayRect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                DrawMaintenanceSections(overlayRect, maintenanceCosts, totalMaintenance);
            }
        }

        private void DrawMaintenanceSections(Rect overlayRect, List<MaintenanceCostInfo> costs, float totalCost)
        {
            if (totalCost <= 0) return;

            float currentX = overlayRect.x;

            for (int i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                float sectionWidth = overlayRect.width * (cost.cost / totalCost);

                Rect sectionRect = new Rect(currentX, overlayRect.y, sectionWidth, overlayRect.height);

                Color sectionColor = Color.cyan;
                GUI.color = sectionColor;
                GUI.DrawTexture(sectionRect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                if (Mouse.IsOver(sectionRect))
                {
                    string tooltip = $"{cost.source}\nMaintenance: {cost.cost:F1}/{cost.interval.ToStringTicksToPeriod()}";
                    TooltipHandler.TipRegion(sectionRect, tooltip);
                }

                currentX += sectionWidth;
            }
        }

        private List<MaintenanceCostInfo> GetMaintenanceCosts()
        {
            var costs = new List<MaintenanceCostInfo>();

            if (basicResourceGene.pawn == null)
                return costs;

            var activeToggleAbilities = basicResourceGene.pawn.abilities?.AllAbilitiesForReading
                ?.OfType<ResourceToggleAbility>()
                ?.Where(x => x != null && x.IsActive && x.ToggleDef != null && x.ToggleDef.resourceMaintainCost > 0)
                ?? Enumerable.Empty<ResourceToggleAbility>();

            foreach (var ability in activeToggleAbilities)
            {
                if (ability.ToggleDef.resourceDef == basicResourceGene.ResourceDef)
                {
                    costs.Add(new MaintenanceCostInfo
                    {
                        source = ability.def.label ?? ability.def.defName,
                        cost = ability.ToggleDef.resourceMaintainCost,
                        interval = ability.ToggleDef.resourceMaintainInterval,
                        isStance = false
                    });
                }
            }

            return costs;
        }

        private List<ResourceData> GetAdditionalResources()
        {
            var additionalResources = new List<ResourceData>();

            if (basicResourceGene.AdditionalResources != null)
            {
                foreach (var extraResource in basicResourceGene.AdditionalResources)
                {
                    if (extraResource.Value != null)
                    {
                        var resourceData = basicResourceGene.GetAdditionalResource(extraResource.Value.resourceDef);
                        if (resourceData != null)
                        {
                            additionalResources.Add(resourceData);
                        }
                    }
                }
            }

            return additionalResources;
        }

        private void DrawAdditionalResourceIcons(Vector2 topLeft, float maxWidth, List<ResourceData> additionalResources)
        {
            float gizmoWidth = GetWidth(maxWidth);
            float iconY = topLeft.y + IconMargin;
            float startX = topLeft.x + gizmoWidth - IconMargin - (additionalResources.Count * IconSpacing);

            for (int i = 0; i < additionalResources.Count; i++)
            {
                var resource = additionalResources[i];
                if (resource?.resourceDef == null)
                    continue;

                float iconX = startX + (i * IconSpacing);
                Rect iconRect = new Rect(iconX, iconY, IconSize, IconSize);

                bool isVisible = basicResourceGene.IsAdditionalResourceVisible(resource.resourceDef);
                Color iconColor = isVisible ? resource.resourceDef.barColor : Color.gray;

                GUI.color = iconColor;
                GUI.DrawTexture(iconRect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                if (Widgets.ButtonImage(iconRect, TexButton.Add))
                {
                    basicResourceGene.ToggleAdditionalResourceVisibility(resource.resourceDef);
                }

                if (Mouse.IsOver(iconRect))
                {
                    string tooltip = $"{resource.resourceDef.label}: {resource.currentValue:F0}/{resource.maxValue:F0}";
                    TooltipHandler.TipRegion(iconRect, tooltip);
                }
            }
        }

        protected override string GetTooltip()
        {
            if (basicResourceGene?.ResourceDef == null)
                return string.Empty;

            var primaryResource = basicResourceGene.ResourceDef;
            string text = $"{primaryResource.label.CapitalizeFirst()}: {basicResourceGene.ValueForDisplay} / {basicResourceGene.MaxForDisplay}\n";

            if (primaryResource.regenStat != null)
            {
                string regen = $"\nRegenerates {primaryResource.RegenStatValue(basicResourceGene.Pawn)} {primaryResource.label.CapitalizeFirst()} every {primaryResource.RegenTicksValue(basicResourceGene.Pawn).ToStringTicksToPeriod()} ticks";
                text += regen;
            }

            if (!basicResourceGene.def.resourceDescription.NullOrEmpty())
            {
                text += $"\n\n{basicResourceGene.def.resourceDescription.Formatted(basicResourceGene.pawn.Named("PAWN")).Resolve()}";
            }

            var maintenanceCosts = GetMaintenanceCosts();
            if (maintenanceCosts.Any())
            {
                text += "\n\nMaintenance Costs:";
                foreach (var cost in maintenanceCosts)
                {
                    string costType = cost.isStance ? "Stance" : "Ability";
                    text += $"\n• {cost.source} ({costType}): {cost.cost:F1} every {cost.interval.ToStringTicksToPeriod()}";
                }

                float totalMaintenance = maintenanceCosts.Sum(x => x.cost);
                text += $"\nTotal: {totalMaintenance:F1} per interval";
            }

            var additionalResources = GetAdditionalResources();
            if (additionalResources.Any())
            {
                text += "\n\nAdditional Resources:";
                foreach (var resource in additionalResources)
                {
                    text += $"\n• {resource.resourceDef.label}: {resource.currentValue:F0}/{resource.maxValue:F0}";
                }
            }

            return text;
        }

        private struct MaintenanceCostInfo
        {
            public string source;
            public float cost;
            public int interval;
            public bool isStance;
        }
    }
}
