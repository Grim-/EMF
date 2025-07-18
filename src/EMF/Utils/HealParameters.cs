using RimWorld;
using System;
using Verse;

namespace EMF
{
    public class HealParameters
    {
        public bool canHealBloodloss = true;
        public bool canHealAddictions = false;
        public bool canHealInfections = false;
        public bool canHealInjuries = true;
        public bool canHealIllnesses = false;
        public bool canHealMissingParts = false;
        public bool canHealPermanentInjuries = true;
        public bool canHealChronicConditions = false;

        public Func<Hediff, bool> CreateFilter()
        {
            return (Hediff h) =>
            {
                //if (!h.def.isBad)
                //{
                //    return false;
                //}

                if (h.def == HediffDefOf.BloodLoss && canHealBloodloss)
                    return true;

                if (h.def.IsAddiction && canHealAddictions)
                    return true;

                if (h.def.isInfection && canHealInfections)
                    return true;

                if (h is Hediff_MissingPart && canHealMissingParts)
                    return true;

                if (h.def.chronic && canHealChronicConditions)
                    return true;

                if (h is Hediff_Injury injury)
                {
                    if (!canHealInjuries)
                        return false;

                    if (injury.IsPermanent() && !canHealPermanentInjuries)
                        return false;

                    return true;
                }

                return false;
            };
        }

        public static HealParameters InjuriesOnly()
        {
            return new HealParameters
            {
                canHealAddictions = false,
                canHealInfections = false,
                canHealInjuries = true,
                canHealIllnesses = false,
                canHealMissingParts = false,
                canHealPermanentInjuries = true,
                canHealChronicConditions = false,
                canHealBloodloss = false
            };
        }

        public static HealParameters AddictionsAndInfectionsOnly()
        {
            return new HealParameters
            {
                canHealAddictions = true,
                canHealInfections = true,
                canHealInjuries = false,
                canHealIllnesses = false,
                canHealMissingParts = false,
                canHealPermanentInjuries = false,
                canHealChronicConditions = false,
                canHealBloodloss = false
            };
        }

        public static HealParameters Everything()
        {
            return new HealParameters
            {
                canHealAddictions = true,
                canHealInfections = true,
                canHealInjuries = true,
                canHealIllnesses = true,
                canHealMissingParts = true,
                canHealPermanentInjuries = true,
                canHealChronicConditions = true,
                canHealBloodloss = true
            };
        }

        public static HealParameters TrollRegeneration()
        {
            return new HealParameters
            {
                canHealAddictions = true,
                canHealInfections = true,
                canHealInjuries = true,
                canHealIllnesses = true,
                canHealMissingParts = true,
                canHealPermanentInjuries = true,
                canHealChronicConditions = false,
                canHealBloodloss = true
            };
        }
    }
}
