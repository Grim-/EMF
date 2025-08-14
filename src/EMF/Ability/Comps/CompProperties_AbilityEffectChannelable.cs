using RimWorld;
using Verse;

namespace EMF
{


    public class CompProperties_AbilityEffectChannelable : CompProperties_AbilityEffect
    {
        public int channelTicks = 2000;
    }

    public class CompAbilityEffectChannelable : CompAbilityEffect, IChannelableAbility
    {
        CompProperties_AbilityEffectChannelable Props => (CompProperties_AbilityEffectChannelable)props;

        protected bool _IsChanneling = false;
        public bool IsChanneling { get => _IsChanneling; set => _IsChanneling = value; }


        protected int _ChannelTicks = 2000;
        public int ChannelTicks { get => _ChannelTicks; set => _ChannelTicks = value; }

        protected bool _ShouldCancel = false;
        public bool ShouldCancel { get => _ShouldCancel; set => _ShouldCancel = value; }

        public Ability Ability => this.parent;

        public virtual void OnChanneBegin(LocalTargetInfo target)
        {
            _ChannelTicks = Props.channelTicks;
        }


        public virtual void OnChanneEnd(LocalTargetInfo target)
        {

        }

        public virtual void OnChannelTick(LocalTargetInfo target)
        {

        }

    }
}
