using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_GrantAbilityOnEquip : CompProperties
    {
        public AbilityDef AbilityToGrant;
        public Dictionary<SkillDef, int> RequiredSkills = new Dictionary<SkillDef, int>();
        public Dictionary<PawnCapacityDef, float> RequiredCapacities = new Dictionary<PawnCapacityDef, float>();
        public Dictionary<StatDef, float> RequiredStats = new Dictionary<StatDef, float>();
        public Dictionary<HediffDef, float> RequiredHediffs = new Dictionary<HediffDef, float>();
        public CompProperties_GrantAbilityOnEquip()
        {
            compClass = typeof(CompGrantAbilityOnEquip);
        }
    }

    public class CompGrantAbilityOnEquip : ThingComp
    {
        private Ability ability;

        public Ability AbilityForReading => ability;

        protected Pawn EquippedPawn = null;

        new CompProperties_GrantAbilityOnEquip Props => (CompProperties_GrantAbilityOnEquip)props;
        private bool DidGrant = false;
        private int? StoredCooldown = null;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            EquippedPawn = pawn;


            if (this.ability == null && this.Props.AbilityToGrant != null)
            {
                this.ability = AbilityUtility.MakeAbility(this.Props.AbilityToGrant, EquippedPawn);
            }

            if (this.ability != null)
            {
                this.ability.pawn = EquippedPawn;
                this.ability.verb.caster = EquippedPawn;
            }

            if (EquippedPawn.Faction == Faction.OfPlayer && !EquippedPawn.HasAbility(AbilityForReading.def) && MeetsRequirements(EquippedPawn))
            {
                EquippedPawn.abilities.abilities.Add(AbilityForReading);
                EquippedPawn.abilities.Notify_TemporaryAbilitiesChanged();
                DidGrant = true;
            }
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (EquippedPawn != null && DidGrant && EquippedPawn.abilities != null && EquippedPawn.abilities.abilities.Contains(AbilityForReading))
            {
                EquippedPawn.abilities.abilities.Remove(AbilityForReading);
                EquippedPawn.abilities.Notify_TemporaryAbilitiesChanged();
                DidGrant = false;
                EquippedPawn = null;
            }
        }
        private bool MeetsRequirements(Pawn pawn)
        {
            if (Props.RequiredSkills.Count > 0)
            {
                foreach (var skillReq in Props.RequiredSkills)
                {
                    if (pawn.skills.GetSkill(skillReq.Key).Level < skillReq.Value)
                    {
                        return false;
                    }
                }
            }

            if (Props.RequiredCapacities.Count > 0)
            {
                foreach (var capacityReq in Props.RequiredCapacities)
                {
                    if (pawn.health.capacities.GetLevel(capacityReq.Key) < capacityReq.Value)
                    {
                        return false;
                    }
                }
            }

            if (Props.RequiredStats.Count > 0)
            {
                foreach (var statReq in Props.RequiredStats)
                {
                    if (pawn.GetStatValue(statReq.Key) < statReq.Value)
                    {
                        return false;
                    }
                }
            }


            if (Props.RequiredHediffs.Count > 0)
            {
                foreach (var hediffreq in Props.RequiredHediffs)
                {
                    if (!pawn.health.hediffSet.HasHediff(hediffreq.Key) || pawn.health.hediffSet.GetFirstHediffOfDef(hediffreq.Key).Severity < hediffreq.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();


            Scribe_References.Look(ref EquippedPawn, "equippedPawn", false);


            Scribe_Deep.Look<Ability>(ref this.ability, "ability");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && EquippedPawn != null && this.AbilityForReading != null)
            {
                this.AbilityForReading.pawn = EquippedPawn;
                this.AbilityForReading.verb.caster = EquippedPawn;
            }
            Scribe_Values.Look(ref DidGrant, "DidGrant");
            Scribe_Values.Look(ref StoredCooldown, "StoredCooldown");
        }
    }
}