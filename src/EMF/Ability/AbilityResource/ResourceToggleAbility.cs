using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class ResourceToggleAbility : ResourceAbility, IToggleableAbility
    {

        public ResourceToggleAbility()
        {

        }

        public ResourceToggleAbility(Pawn pawn) : base(pawn)
        {

        }

        public ResourceToggleAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept)
        {

        }

        public ResourceToggleAbility(Pawn pawn, AbilityDef def) : base(pawn, def)
        {

        }

        public ResourceToggleAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def)
        {

        }
        new public ResourceToggleAbilityDef ResourceDef => (ResourceToggleAbilityDef)def;

        protected bool _IsActive = false;

        public bool IsActive
        {
            get => _IsActive;
        }

        public override AcceptanceReport CanCast
        {
            get
            {
                //if it has a cooldown and the toggle is active, allow deactivating regardless of cooldown
                if (IsActive)
                {
                    return true;
                }
                else return base.CanCast;
            }
        }

        public override bool CanQueueCast
        {
            get
            {
                //if it has a cooldown and the toggle is active, allow deactivating regardless of cooldown
                if (IsActive)
                {
                    return true;
                }
                else return base.CanQueueCast;
            }
        }

        public override string Tooltip
        {
            get
            {

                if (ResourceDef != null && ResourceDef.resourceDef != null)
                {
                    return base.Tooltip + $"\r\nMaintain Cost : {ResourceDef.resourceMaintainCost} ({ResourceDef.resourceDef.LabelCap}) every {ResourceDef.resourceMaintainInterval.ToStringTicksToPeriod()}.";
                }

                return base.Tooltip;
            }
        }

        public ResourceToggleAbilityDef ToggleDef => ResourceDef;

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (IsActive)
            {
                DeActivate();
                return false;
            }
            else
            {
                if (resourceGene.Has(ResourceDef.resourceDef, ResourceDef.resourceCost))
                {
                    resourceGene.Consume(ResourceDef.resourceDef, ResourceDef.resourceCost);
                    Activate();
                    return false;
                }
                return false;
            }
        }

        public override void AbilityTick()
        {
            base.AbilityTick();

            if (_IsActive)
            {
                if (this.pawn.IsHashIntervalTick(ToggleDef.resourceMaintainInterval))
                {
                    if (!resourceGene.Has(ResourceDef.resourceDef, ResourceDef.resourceMaintainCost))
                    {
                        DeActivate();
                    }
                    else
                    {
                        resourceGene.Consume(ResourceDef.resourceDef, ResourceDef.resourceMaintainCost);
                    }
                }
            }
        }

        protected override void ConsumeResource()
        {
            //if (_IsActive)
            //{
            //    base.ConsumeResource();
            //}  
        }


        public void Activate(bool force = false)
        {
            if (_IsActive && !force)
            {
                return;
            }

            _IsActive = true;
            OnActivated();
        }

        public void DeActivate(bool force = false)
        {
            if (!_IsActive && !force)
            {
                return;
            }

            _IsActive = false;
            OnDeactivated();
        }


        protected virtual void OnActivated()
        {
            foreach (var item in this.CompsOfType<CompAbilityEffect_Toggleable>())
            {
                item.OnToggleOn();
            }
        }

        protected virtual void OnDeactivated()
        {
            foreach (var item in this.CompsOfType<CompAbilityEffect_Toggleable>())
            {
                item.OnToggleOff();
            }
        }


        public override IEnumerable<Command> GetGizmos()
        {
            //return base.GetGizmos();
            if (this.gizmo == null)
            {
                this.gizmo = new Command_ToggleAbility(this, this.pawn);
                this.gizmo.Order = this.def.uiOrder;
            }

            yield return gizmo;
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (_IsActive)
            {
                reason = "";
                return false;
            }
            return base.GizmoDisabled(out reason);
        }
    }
}
