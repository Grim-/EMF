using System.Linq;
using Verse;

namespace EMF
{
    public static class BoundWeaponUtil
    {
        public static Thing GetBoundWeapon(this Pawn pawn)
        {
            HediffComp_BoundWeapon boundWeapon = pawn.GetBoundWeaponComp();

            if (boundWeapon != null && boundWeapon.HasBoundThing)
            {
                return boundWeapon.BoundThing;
            }
            return null;
        }

        public static bool BindWeaponTo(this Pawn pawn, Thing thingToBind)
        {
            Hediff boundHediff = pawn.health.GetOrAddHediff(EMFDefOf.EMF_BoundWeapon);
            HediffComp_BoundWeapon boundWeapon = boundHediff.TryGetComp<HediffComp_BoundWeapon>();

            if (boundWeapon != null)
            {
                if (boundWeapon.HasBoundThing)
                {
                    return false;
                }

                boundWeapon.SetBoundWeapon(thingToBind);

                return true;
            }
            return false;
        }
        public static bool UnbindWeapon(this Pawn pawn)
        {
            HediffComp_BoundWeapon boundWeapon = pawn.GetBoundWeaponComp();
            if (boundWeapon != null && boundWeapon.HasBoundThing)
            {
                boundWeapon.ClearBinding();
                return true;
            }
            return false;
        }
        public static bool IsBoundTo(this Thing thing, Pawn pawn)
        {
            if (thing == null || pawn == null) return false;

            var boundWeapon = pawn.GetBoundWeapon();
            return boundWeapon == thing;
        }

        public static bool HasBoundWeapon(this Pawn pawn)
        {
            return pawn.GetBoundWeapon() != null;
        }

        public static HediffComp_BoundWeapon GetBoundWeaponComp(this Pawn pawn)
        {
            Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_BoundWeapon>() != null);
            if (hediff != null)
            {
                HediffComp_BoundWeapon boundWeapon = hediff.TryGetComp<HediffComp_BoundWeapon>();

                if (boundWeapon != null)
                {
                    return boundWeapon;
                }
            }
            return null;
        }
    }
}
