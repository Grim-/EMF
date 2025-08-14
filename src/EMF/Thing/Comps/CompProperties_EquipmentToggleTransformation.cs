using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_EquipmentToggleTransformation : CompProperties_BasePawnComp
    {
        public HediffDef hediffDef;
        public int damagePerUse = 0;

        public CompProperties_EquipmentToggleTransformation()
        {
            compClass = typeof(Comp_EquipmentToggleTransformation);
        }
    }

    public class Comp_EquipmentToggleTransformation : Comp_BasePawnComp
    {
        private CompProperties_EquipmentToggleTransformation Props => (CompProperties_EquipmentToggleTransformation)props;
        protected Hediff AppliedHediff = null;
        protected bool IsApplied => AppliedHediff != null;
        public override void Notify_Unequipped(Pawn pawn)
        {
            if (IsApplied)
            {
                Disable();
            }
            base.Notify_Unequipped(pawn);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (IsApplied)
            {
                Disable();
            }
            base.PostDestroy(mode, previousMap);
        }

        public void Enable()
        {
            if (EquippedPawn != null)
            {
                if (!EquippedPawn.health.hediffSet.HasHediff(Props.hediffDef))
                {
                    AppliedHediff = (HediffWithComps)HediffMaker.MakeHediff(Props.hediffDef, EquippedPawn);
                    var transformComp = AppliedHediff.TryGetComp<HediffComp_Transformation>();
                    if (transformComp != null)
                    {
                        transformComp.SetTransformationSource(parent);
                    }

                    EquippedPawn.health.hediffSet.AddDirect(AppliedHediff);

                    if (Props.damagePerUse > 0)
                    {
                        if (this.parent.def.useHitPoints)
                        {
                            this.parent.HitPoints -= Props.damagePerUse;
                        }
                    }
                }
            }
        }
        public void Disable()
        {
            if (EquippedPawn != null)
            {
                if (AppliedHediff != null)
                {
                    EquippedPawn.health.RemoveHediff(AppliedHediff);
                    AppliedHediff = null;
                }
            }
        }
        public void Toggle()
        {
            if (IsApplied)
            {
                Disable();
            }
            else Enable();
        }
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            yield return new Command_Toggle()
            {
                defaultLabel = IsApplied ? "Deactivate" : "Activate",
                icon = this.parent.def.GetUIIconForStuff(this.parent.Stuff),
                defaultDesc = $"Enable or disable {Props.hediffDef.LabelCap}" + (Props.damagePerUse > 0 ? $" and causes {Props.damagePerUse} each time it is activated" : ""),
                toggleAction = () =>
                {
                    Toggle();
                },
                isActive = () => IsApplied
            };
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref AppliedHediff, "appliedHediff");
        }
    }
}
