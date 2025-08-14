using UnityEngine;
using Verse;

namespace EMF
{
    public class ResourceToggleAbilityDef : ResourceAbilityDef
    {
        public string activeLabel = "Active";
        public string inActiveLabel = "Inactive";

        public string customEnabledTexPath = string.Empty;
        public string customDisabledTexPath = string.Empty;

        public Texture2D ToggleEnabledTex => string.IsNullOrEmpty(customEnabledTexPath)?  Widgets.CheckboxOnTex : ContentFinder<Texture2D>.Get(customEnabledTexPath);
        public Texture2D ToggleDisabledTex => string.IsNullOrEmpty(customDisabledTexPath) ? Widgets.CheckboxOffTex : ContentFinder<Texture2D>.Get(customDisabledTexPath);


        public float resourceMaintainCost = 0;
        public int resourceMaintainInterval = 300;

        public ResourceToggleAbilityDef()
        {
            abilityClass = typeof(ResourceToggleAbility);
        }
    }
}
