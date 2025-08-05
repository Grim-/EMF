using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_EquipmentToggleHediff : CompProperties_BasePawnComp
    {
        public HediffDef hediffDef;
        public CompProperties_EquipmentToggleHediff()
        {
            compClass = typeof(Comp_EquipmentToggleHediff);
        }
    }

    public class Comp_EquipmentToggleHediff : Comp_BasePawnComp
    {
        private CompProperties_EquipmentToggleHediff Props => (CompProperties_EquipmentToggleHediff)props;
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
                defaultLabel = IsApplied ? "Remove" : "Apply",
                icon = this.parent.def.GetUIIconForStuff(this.parent.Stuff),
                defaultDesc = $"Enable or disable {Props.hediffDef.LabelCap}",
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
