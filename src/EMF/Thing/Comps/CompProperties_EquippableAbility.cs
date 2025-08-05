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
        protected List<Ability> grantedAbilities = new List<Ability>();
        protected Dictionary<AbilityDef, int> cooldownTicks = new Dictionary<AbilityDef, int>();

        protected List<AbilityDef> WorkingKeys = new List<AbilityDef>();
        protected List<int> WorkingValues = new List<int>();

        private CompProperties_EquippableAbility Props => (CompProperties_EquippableAbility)props;
        public Pawn EquipOwner = null;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            EquipOwner = pawn;
            RefreshGrantedAbilities(pawn);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            RemoveGrantedAbilities(pawn);
            base.Notify_Unequipped(pawn);
            EquipOwner = null;
        }

        private void RefreshGrantedAbilities(Pawn pawn)
        {
            GrantAvailableAbilities(pawn);
        }

        private void GrantAvailableAbilities(Pawn pawn)
        {
            if (pawn.abilities == null)
            {
                return;
            }

            if (Props.abilityDefs == null)
            {
                return;
            }


            foreach (var levelAbility in Props.abilityDefs)
            {
                pawn.abilities.GainAbility(levelAbility);
                var ability = pawn.abilities.GetAbility(levelAbility);

                if (cooldownTicks.ContainsKey(levelAbility) && cooldownTicks[levelAbility] > 0)
                {
                    ability.StartCooldown(cooldownTicks[levelAbility]);
                }

                grantedAbilities.Add(ability);
            }
        }



        private void RemoveGrantedAbilities(Pawn pawn)
        {
            if (pawn != null)
            {
                foreach (var ability in grantedAbilities)
                {
                    cooldownTicks[ability.def] = ability.CooldownTicksRemaining;
                    pawn.abilities.RemoveAbility(ability.def);
                }
            }

            grantedAbilities.Clear();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (EquipOwner != null)
            {
                RemoveGrantedAbilities(EquipOwner);
            } 
            base.PostDestroy(mode, previousMap);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref grantedAbilities, "grantedAbilities", LookMode.Reference);
            Scribe_Collections.Look(ref cooldownTicks, "cooldownTicks", LookMode.Def, LookMode.Value, ref WorkingKeys, ref WorkingValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && cooldownTicks == null)
            {
                cooldownTicks = new Dictionary<AbilityDef, int>();
            }
        }
    }
}
