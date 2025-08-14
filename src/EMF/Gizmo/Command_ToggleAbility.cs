using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EMF
{
    [StaticConstructorOnStartup]
    public class Command_ToggleAbility : Command_Ability
    {
        private static readonly Color ToggleOnColor = new Color(0.2f, 1f, 0.2f, 0.25f);
        private static readonly Color ToggleOffColor = new Color(1f, 0.2f, 0.2f, 0.25f);

        private ResourceToggleAbility toggleAbility;

        public bool IsOn
        {
            get
            {
                return toggleAbility?.IsActive ?? false;
            }
        }

        public Command_ToggleAbility(ResourceToggleAbility ability, Pawn pawn) : base(ability, pawn)
        {
            this.toggleAbility = ability;
            UpdateToggleStatus();
        }

        private void UpdateToggleStatus()
        {
            if (IsOn)
            {
                this.defaultDesc = this.Tooltip + "\n\n" + "ClickToToggleOff".Translate();
            }
            else
            {
                this.defaultDesc = this.Tooltip + "\n\n" + "ClickToToggleOn".Translate();
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            UpdateToggleStatus();

            Rect rect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            GizmoResult baseResult = base.GizmoOnGUI(topLeft, maxWidth, parms);

            if (IsOn)
            {
                Rect toggleRect = rect.ContractedBy(1f);
                GUI.color = ToggleOnColor;
                GUI.DrawTexture(toggleRect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                Rect iconRect = new Rect(rect.x + rect.width - 24f, rect.y + 2f, 20f, 20f);
                Texture2D toggleTex = IsOn ? toggleAbility.ToggleDef.ToggleEnabledTex : toggleAbility.ToggleDef.ToggleDisabledTex;
                GUI.DrawTexture(iconRect, toggleTex);
            }

            return baseResult;
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            UpdateToggleStatus();

            GizmoResult result = base.GizmoOnGUIInt(butRect, parms);

            if (!this.disabled && IsOn)
            {
                GUI.color = ToggleOnColor;
                GUI.DrawTexture(butRect.ContractedBy(1f), BaseContent.WhiteTex);
                GUI.color = Color.white;
            }

            return result;
        }

        protected override void DisabledCheck()
        {
            if (IsOn)
            {
                this.disabled = false;
                this.disabledReason = null;
            }
            else
            {
                base.DisabledCheck();
            }
        }

        public override void ProcessInput(Event ev)
        {
            if (!toggleAbility.def.targetRequired)
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);

                if (IsOn)
                {
                    toggleAbility.DeActivate();
                    SoundDefOf.Designate_Cancel.PlayOneShotOnCamera(null);
                }
                else
                {
                    if (toggleAbility.CanCast)
                    {
                        toggleAbility.QueueCastingJob(toggleAbility.pawn, LocalTargetInfo.Invalid);
                        SoundDefOf.Designate_PlanAdd.PlayOneShotOnCamera(null);
                    }
                }

                UpdateToggleStatus();
            }
            else
            {
                base.ProcessInput(ev);
            }
        }

        public override string TopRightLabel
        {
            get
            {
                string baseLabel = base.TopRightLabel;
                if (IsOn)
                {
                    if (!string.IsNullOrEmpty(baseLabel))
                    {
                        return baseLabel + " [ON]";
                    }
                    return "[ON]";
                }
                return baseLabel;
            }
        }
    }
}
