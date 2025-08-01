using Verse;

namespace EMF
{
    public class CompProperties_AbilityEffectChannelableBeam : CompProperties_AbilityEffectChannelable
    {
        public BeamParameters beamParameters;

        public CompProperties_AbilityEffectChannelableBeam()
        {
            compClass = typeof(CompAbilityEffectChannelableBEam);
        }
    }

    public class CompAbilityEffectChannelableBEam : CompAbilityEffectChannelable, IChannelableAbility
    {
        protected Beam_Thing Beam = null;
        CompProperties_AbilityEffectChannelableBeam Props => (CompProperties_AbilityEffectChannelableBeam)props;
        public override void OnChanneBegin(LocalTargetInfo target)
        {
            if (Beam != null)
            {
                if (!Beam.Destroyed)
                    Beam.Destroy();
            }

            Beam = Beam_Thing.Create(this.parent.pawn, target, this.parent, Props.beamParameters);

            GenSpawn.Spawn(Beam, this.parent.pawn.Position, this.parent.pawn.Map);
        }


        public override void OnChanneEnd(LocalTargetInfo target)
        {
            if (Beam != null)
            {
                if (!Beam.Destroyed)
                    Beam.Destroy();
            }
        }

        public override void OnChannelTick(LocalTargetInfo target)
        {

        }

    }
}
