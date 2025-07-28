using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class HediffCompProperties_DamageReduction : HediffCompProperties
    {
        public List<DamageReduction> damageReductions = new List<DamageReduction>();

        public HediffCompProperties_DamageReduction()
        {
            compClass = typeof(HediffComp_DamageReduction);
        }
    }
    public class DamageReduction
    {
        public DamageDef damageDef;
        public float damageFactor = 1f;
        public float flatReduction = 0f;
    }
    public class HediffComp_DamageReduction : HediffComp_OnDamageTakenEffect
    {
        public HediffCompProperties_DamageReduction Props => (HediffCompProperties_DamageReduction)props;

        protected override bool OnBeforeThingDamageTaken(ref DamageInfo dinfo)
        {
            if (!Props.damageReductions.NullOrEmpty())
            {
                foreach (DamageReduction reduction in Props.damageReductions)
                {
                    if (reduction.damageDef == dinfo.Def)
                    {
                        float originalAmount = dinfo.Amount;
                        float newAmount = originalAmount * reduction.damageFactor - reduction.flatReduction;
                        newAmount = Mathf.Max(0f, newAmount);

                        dinfo.SetAmount(newAmount);

                        return newAmount <= 0f;
                    }
                }
            }

            return false;
        }
    }

}