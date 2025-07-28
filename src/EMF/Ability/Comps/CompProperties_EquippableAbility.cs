using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_EquippableAbility : CompProperties
    {
        public List<AbilityDef> abilityDefs = new List<AbilityDef>();
        public CompProperties_EquippableAbility()
        {
            this.compClass = typeof(CompEquippableAbility);
        }

    }
    public class CompEquippableAbility : ThingComp
    {
        private CompProperties_EquippableAbility Props
        {
            get
            {
                return this.props as CompProperties_EquippableAbility;
            }
        }

        private List<Ability> abilities = new List<Ability>();

        public List<Ability> AbilitiesForReading
        {
            get
            {
                if (this.abilities == null || this.abilities.Count == 0)
                {
                    this.abilities = new List<Ability>();
                    foreach (AbilityDef abilityDef in this.Props.abilityDefs)
                    {
                        this.abilities.Add(AbilityUtility.MakeAbility(abilityDef, EquippedPawn));
                    }
                }
                return this.abilities;
            }
        }

        protected Pawn EquippedPawn = null;

        public override void Notify_Equipped(Pawn pawn)
        {
            EquippedPawn = pawn;
            foreach (AbilityDef abilityDef in Props.abilityDefs)
            {
                if (pawn.abilities.GetAbility(abilityDef) == null)
                {
                    Ability ability = this.AbilitiesForReading.FirstOrDefault(a => a.def == abilityDef);
                    if (ability != null)
                    {
                        ability.pawn = EquippedPawn;
                        ability.verb.caster = EquippedPawn;
                        pawn.abilities.GainAbility(abilityDef);
                    }
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            foreach (AbilityDef abilityDef in Props.abilityDefs)
            {
                if (pawn.abilities.GetAbility(abilityDef) != null)
                {
                    pawn.abilities.RemoveAbility(abilityDef);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.Saving && this.abilities == null)
            {
                this.abilities = new List<Ability>();
            }

            Scribe_Collections.Look(ref this.abilities, "abilities", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && EquippedPawn != null)
            {
                if (this.abilities == null)
                {
                    this.abilities = new List<Ability>();
                }

                foreach (Ability ability in this.AbilitiesForReading)
                {
                    ability.pawn = EquippedPawn;
                    ability.verb.caster = EquippedPawn;
                }
            }
        }
    }
}
