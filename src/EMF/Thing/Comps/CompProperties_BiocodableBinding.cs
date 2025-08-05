using Verse;

namespace EMF
{


    public class CompProperties_BiocodableBinding : CompProperties_SelectiveBiocodable
    {
        public bool automaticallyBind = true;
        public bool removeBindingWhenUnbiocoded = true;

        public CompProperties_BiocodableBinding()
        {
            compClass = typeof(CompBiocodableBinding);
        }
    }

    public class CompBiocodableBinding : CompSelectiveBiocodable
    {
        public CompProperties_BiocodableBinding BindingProps => (CompProperties_BiocodableBinding)props;

        public override void CodeFor(Pawn p)
        {
            base.CodeFor(p);

            if (Biocoded && BindingProps.automaticallyBind)
            {
                BindToPawn(p);
            }
        }

        public override void UnCode()
        {
            if (BindingProps.removeBindingWhenUnbiocoded)
            {
                UnbindFromPawn(CodedPawn);
            }

            base.UnCode();
        }

        private void BindToPawn(Pawn pawn)
        {
            if (pawn == null || !Biocoded || CodedPawn != pawn) return;

            pawn.BindWeaponTo(parent);
        }

        private void UnbindFromPawn(Pawn pawn)
        {
            if (pawn == null) return;

            pawn.UnbindWeapon();
        }

        public override string CompInspectStringExtra()
        {
            var baseString = base.CompInspectStringExtra();

            if (Biocoded && CodedPawn != null)
            {
                var boundWeapon = CodedPawn.GetBoundWeapon();
                if (boundWeapon == parent)
                {
                    return baseString + "\nBound to " + CodedPawn.LabelShort;
                }
            }

            return baseString;
        }
    }
}
