using RimWorld;
using Verse;

namespace EMF
{
    public interface IChannelableAbility
    {

        Ability Ability { get; }

        bool IsChanneling { get; set; }
        int ChannelTicks { get; set; }
        bool ShouldCancel { get; set; }

        void OnChannelTick(LocalTargetInfo target);
        void OnChanneBegin(LocalTargetInfo target);
        void OnChanneEnd(LocalTargetInfo target);
    }
}