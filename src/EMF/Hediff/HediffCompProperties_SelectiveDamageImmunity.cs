using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_SelectiveDamageImmunity : HediffCompProperties
    {
        public List<DamageTypeModifiers> vulnerableToDamageTypes;

        public HediffCompProperties_SelectiveDamageImmunity()
        {
            compClass = typeof(HediffComp_SelectiveDamageImmunity);
        }
    }
    public class HediffComp_SelectiveDamageImmunity : HediffComp
    {
        public new HediffCompProperties_SelectiveDamageImmunity Props => (HediffCompProperties_SelectiveDamageImmunity)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            EventManager.Instance.OnBeforeThingDamageTaken += Instance_OnBeforeThingDamageTaken;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            EventManager.Instance.OnBeforeThingDamageTaken -= Instance_OnBeforeThingDamageTaken;
        }

        private bool Instance_OnBeforeThingDamageTaken(Thing target, ref DamageInfo dinfo)
        {
            if (target != this.parent.pawn)
            {
                return false;
            }

            if (!IsVulnerableToDamage(dinfo.Def))
            {
                dinfo.SetAmount(0);
                dinfo.SetAllowDamagePropagation(false);
                return true;
            }

            return false;
        }

        public bool IsVulnerableToDamage(DamageDef damageDef)
        {
            return Props.vulnerableToDamageTypes != null &&
                   Props.vulnerableToDamageTypes.Any(x => x.DamageType == damageDef);
        }

        public float GetDamageMod(DamageDef damageType)
        {
            if (Props.vulnerableToDamageTypes == null)
                return 1f;

            var modifier = Props.vulnerableToDamageTypes.FirstOrDefault(x => x.DamageType == damageType);
            return modifier != null ? modifier.DamageModifier : 1f;
        }


        public DamageInfo ModifyDamage(DamageInfo DamageInfo)
        {
            float damageMod = GetDamageMod(DamageInfo.Def);
            DamageInfo.SetAmount(DamageInfo.Amount * damageMod);
            return DamageInfo;
        }
    }

    public class DamageTypeModifiers
    {
        public DamageDef DamageType;
        public float DamageModifier = 0f;
    }
}