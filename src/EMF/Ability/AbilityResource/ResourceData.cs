using UnityEngine;
using Verse;

namespace EMF
{
    public class ResourceData : IExposable
    {
        public AbilityResourceDef resourceDef;
        public float currentValue = 0f;
        public float maxValue = 100f;
        public float targetValue = 0.5f;
        public bool isLocked = false;
        public bool isRegenEnabled = false;
        public int currentRegenTick = 0;
        public float totalResourceUsed = 0;
        public bool enableResource = true;



        public string ValueDisplayString => $"{currentValue} / {maxValue}";

        public float ValueAsPercent => Mathf.Clamp01(currentValue / maxValue);

        public ResourceData()
        {
        }

        public ResourceData(AbilityResourceDef def, float initialMax)
        {
            resourceDef = def;
            maxValue = initialMax;
            currentValue = initialMax;
            targetValue = initialMax * 0.5f;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref resourceDef, "resourceDef");
            Scribe_Values.Look(ref currentValue, "currentValue", 0f);
            Scribe_Values.Look(ref maxValue, "maxValue", 0f);
            Scribe_Values.Look(ref targetValue, "targetValue", 0.5f);
            Scribe_Values.Look(ref isLocked, "isLocked", false);
            Scribe_Values.Look(ref isRegenEnabled, "isRegenEnabled", false);
            Scribe_Values.Look(ref currentRegenTick, "currentRegenTick", 0);
            Scribe_Values.Look(ref totalResourceUsed, "totalResourceUsed", 0f);
            Scribe_Values.Look(ref enableResource, "enableResource", true);
        }
    }
}
