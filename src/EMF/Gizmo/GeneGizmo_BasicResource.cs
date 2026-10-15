using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{

    [StaticConstructorOnStartup]
    public class Gizmo_AbilityResource : Gizmo_Slider
    {
        private readonly IAbilityResourceContainer container;
        private readonly string label;
        private readonly Func<bool> draggableGetter;
        private readonly Func<float> targetGetter;
        private readonly Action<float> targetSetter;

        private bool draggingBar;

        public Gizmo_AbilityResource(
            IAbilityResourceContainer container,
            string label,
            Func<bool> draggableGetter = null,
            Func<float> targetGetter = null,
            Action<float> targetSetter = null)
        {
            this.container = container;
            this.label = label;
            this.draggableGetter = draggableGetter;
            this.targetGetter = targetGetter;
            this.targetSetter = targetSetter;
        }


        protected override bool DraggingBar
        {
            get => draggingBar;
            set => draggingBar = value;
        }

        protected override bool IsDraggable
        {
            get
            {
                return draggableGetter != null && draggableGetter();
            }
        }

        protected override float Target
        {
            get => targetGetter != null ? targetGetter() : 0f;
            set => targetSetter?.Invoke(value);
        }

        protected override string Title => label;
        protected override string BarLabel => $"{container.CurrentValue:F0} / {container.MaxValue:F0}";

        protected override float ValuePercent => container.MaxValue > 0 ? container.CurrentValue / container.MaxValue : 0f;

        protected override Color BarColor => container.ResourceDef?.barColor ?? Color.white;
        protected override Color BarHighlightColor => container.ResourceDef?.barHighlightColor ?? Color.grey;

        protected override FloatRange DragRange => new FloatRange(0f, 1f);

        protected override int Increments => 10;

        protected override IEnumerable<float> GetBarThresholds()
        {
            yield break;
        }

        protected override string GetTooltip()
        {
            if (container == null || container.ResourceDef == null) return string.Empty;

            string text = $"{container.ResourceDef.LabelCap}: {container.CurrentValue:F0} / {container.MaxValue:F0}\n";
            if (!container.ResourceDef.description.NullOrEmpty())
            {
                text += $"\n{container.ResourceDef.description}";
            }
            return text;
        }
    }



    [StaticConstructorOnStartup]
    public class GeneGizmo_BasicResource : GeneGizmo_Resource
    {
        private ResourceData ResourceData;
        protected override string Title
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return ResourceData.resourceDef.LabelCap;
                }
                return base.Title;
            }
        }


        private bool IsDraggingBar = false;
        protected override bool DraggingBar { get => IsDraggingBar; set => IsDraggingBar = value; }


        public GeneGizmo_BasicResource(Gene_BasicResource gene, ResourceData data, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor) : base(gene, drainGenes, barColor, barHighlightColor)
        {
            if (gene == null)
            {
                Log.Error("GeneGizmo_BasicResource created with null gene");
                return;
            }
            ResourceData = data;
        }

        public GeneGizmo_BasicResource(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor) : base(gene, drainGenes, barColor, barHighlightColor)
        {

        }
        protected override float ValuePercent
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return basicResource.GetAdditionalResource(ResourceData.resourceDef).ValueAsPercent;
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
                    return basicResource.GetAdditionalResource(ResourceData.resourceDef).ValueDisplayString;
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
                    return ResourceData.resourceDef.barColor;
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
                    return ResourceData.resourceDef.barHighlightColor;
                }

                return base.BarHighlightColor;
            }
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (var item in base.RightClickFloatMenuOptions)
                {
                    yield return item;
                }

                yield return new FloatMenuOption("Im ahwoih", () =>
                {


                });
            }
        }


        protected override string GetTooltip()
        {
            if (!(gene is Gene_BasicResource resourceGene))
                return string.Empty;

            string text = $"{ResourceData.resourceDef.LabelCap}: {resourceGene.GetAdditionalResource(ResourceData.resourceDef).currentValue} / {resourceGene.GetAdditionalResource(ResourceData.resourceDef).maxValue}\n";

            if (ResourceData.isRegenEnabled)
            {
                text += $"\nRegenerates {ResourceData.resourceDef.RegenStatValue(this.gene.pawn)} {ResourceData.resourceDef.LabelCap} every {ResourceData.resourceDef.RegenTicksValue(this.gene.pawn)} ticks.";
            }

            if (!resourceGene.def.resourceDescription.NullOrEmpty())
            {
                text += $"\n\n{ResourceData.resourceDef.description.Formatted(resourceGene.pawn.Named("PAWN")).Resolve()}";
            }
            return text;
        }
    }
}
