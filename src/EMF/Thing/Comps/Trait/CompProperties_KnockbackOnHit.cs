using RimWorld;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_KnockbackOnHit : CompProperties
    {
        public float knockbackChance = 0.5f;
        public IntRange knockbackRange = new IntRange(2, 3);
        public float collisionDamage = 5f;

        public CompProperties_KnockbackOnHit()
        {
            compClass = typeof(CompKnockbackOnHit);
        }
    }

    public class CompKnockbackOnHit : BaseTraitComp
    {
        public CompProperties_KnockbackOnHit Props => (CompProperties_KnockbackOnHit)props;

        public override string TraitName => "Knockback";

        public override string Description =>
            $"This equipment has a chance ({Mathf.RoundToInt(Props.knockbackChance * 100)}%) to knock back a target on melee attack, upto {Props.knockbackRange.max} tiles away.";

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, ref DamageWorker.DamageResult damageWorkerResult)
        {
            Map map = attacker.Map;

            if (target.Pawn != null)
            {
                IntVec3 currentTargetPosition = target.Pawn.Position;

                if (!target.Pawn.DeadOrDowned && Rand.Range(0, 1) <= Props.knockbackChance)
                {
                    IntVec3 launchDirection = target.Pawn.Position - parent.Position;
                    IntVec3 destination = target.Pawn.Position + launchDirection * Rand.Range(1, 2);
                    destination = destination.ClampInsideMap(map);

                    if (destination.IsValid && destination.InBounds(map) && !destination.Fogged(map) && target.Pawn.Faction.HostileTo(parent.Faction))
                    {
                        ThingFlyer pawnFlyer = ThingFlyer.MakeFlyer(target.Pawn, destination, map, null, null, null, null, false);

                        ThingFlyer.LaunchFlyer(pawnFlyer, target.Pawn, currentTargetPosition, map);
                    }
                }
            }

            return base.Notify_ApplyMeleeDamageToTarget(target, attacker, ref damageWorkerResult);
        }

    }
}
