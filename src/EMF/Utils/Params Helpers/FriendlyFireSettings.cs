using RimWorld;
using Verse;

namespace EMF
{
    public class FriendlyFireSettings
    {
        public bool canTargetHostile = true;
        public bool canTargetFriendly = true;
        public bool canTargetNeutral = true;
        public bool canTargetSelf = false;


        public bool CanTargetThing(Thing targetThing, Thing attackerThing)
        {
            if (attackerThing == null)
            {
                Log.Error("Attack is null for FriendlyFireSettings : CanTargetThing");
                return false;
            }

            if (targetThing == null)
            {
                Log.Error("Target is null for FriendlyFireSettings : CanTargetThing");
                return false;
            }

            if (!canTargetSelf && targetThing == attackerThing)
            {
                return false;
            }

            if (targetThing.Faction == null)
                return canTargetNeutral;

            Faction sourceFaction = attackerThing.Faction;

            if (targetThing.Faction == sourceFaction && canTargetFriendly)
                return true;

            if (canTargetHostile && targetThing.Faction.HostileTo(sourceFaction))
                return true;

            if (canTargetNeutral && !targetThing.Faction.HostileTo(sourceFaction) && targetThing.Faction != sourceFaction)
                return true;

            return false;
        }

        public static FriendlyFireSettings AllFriendly()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = true,
                canTargetHostile = false,
                canTargetNeutral = true,
                canTargetSelf = true
            };
        }

        public static FriendlyFireSettings FriendlyFactionOnly()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = true,
                canTargetHostile = false,
                canTargetNeutral = false,
                canTargetSelf = true
            };
        }

        public static FriendlyFireSettings HostileOnly()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = false,
                canTargetHostile = true,
                canTargetNeutral = false,
                canTargetSelf = false
            };
        }
        public static FriendlyFireSettings AllAttack()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = false,
                canTargetHostile = true,
                canTargetNeutral = true,
                canTargetSelf = false
            };
        }
        public static FriendlyFireSettings All()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = true,
                canTargetHostile = true,
                canTargetNeutral = true,
                canTargetSelf = true
            };
        }
    }
}
