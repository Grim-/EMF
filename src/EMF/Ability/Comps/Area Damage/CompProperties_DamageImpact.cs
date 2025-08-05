using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_DamageImpact : CompProperties_BaseJumpEffect
    {
        public DamageParameters damageParameters = DamageParameters.Default();
        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();

        public CompProperties_DamageImpact()
        {
            compClass = typeof(CompAbilityEffect_DamageImpact);
        }
    }

    public class CompAbilityEffect_DamageImpact : CompAbilityEffect_BaseJumpEffect
    {
        CompProperties_DamageImpact Props => (CompProperties_DamageImpact)props;

        protected override void OnLand(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            base.OnLand(arg1, arg2, arg3);

            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.landingRadius, true).ToList();

            foreach (IntVec3 cell in cells)
            {
                List<Thing> thingsInCell = cell.GetThingList(arg3.Map).ToList();

                foreach (Thing thing in thingsInCell)
                {
                    if (!Props.friendlyFireSettings.CanTargetThing(thing, this.parent.pawn))
                        continue;

                    Props.damageParameters.DealDamageTo(this.parent.pawn, thing);
                }
            }
        }
    }


}
