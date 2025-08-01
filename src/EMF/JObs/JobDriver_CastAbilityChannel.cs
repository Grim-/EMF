using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EMF
{
    public class JobDriver_CastAbilityChannel : JobDriver
    {
        public override string GetReport()
        {
            return "Channeling Ability".Translate(job.ability.def.label);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return TargetA.HasThing && pawn.Reserve(TargetA.Thing, job) || TargetA.IsValid;
        }

        public Job Job => job;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var ability = Job.ability;
            var effectComps = ability.EffectComps;

            bool hasChannelableComp = false;
            foreach (var comp in effectComps)
            {
                if (comp is IChannelableAbility)
                {
                    hasChannelableComp = true;
                    break;
                }
            }

            if (!hasChannelableComp)
            {
                Log.Error($"Ability {ability.def.defName} has no components implementing IChannelableAbility but is using JobDriver_CastAbilityChannel");
                yield break;
            }

            yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, false);


            yield return Toils_General.Do(delegate
            {
                foreach (var comp in effectComps)
                {
                    if (comp is IChannelableAbility channelable)
                    {
                        channelable.IsChanneling = true;
                        channelable.ShouldCancel = false;
                        channelable.OnChanneBegin(job.targetA);
                    }
                }
            });

            int maxChannelTicks = 0;
            foreach (var comp in effectComps)
            {
                if (comp is IChannelableAbility channelable)
                {
                    maxChannelTicks = Mathf.Max(maxChannelTicks, channelable.ChannelTicks);
                }
            }

            if (maxChannelTicks > 0)
            {
                var channelToil = Toils_General.Wait(maxChannelTicks, TargetIndex.A)
                    .WithProgressBarToilDelay(TargetIndex.C);

                channelToil.tickAction = delegate
                {
                    foreach (var comp in effectComps)
                    {
                        if (comp is IChannelableAbility channelable)
                        {
                            channelable.OnChannelTick(job.targetA);

                            if (channelable.ShouldCancel)
                            {
                                channelable.IsChanneling = false;
                                channelable.OnChanneEnd(job.targetA);
                                this.EndJobWith(JobCondition.InterruptForced);
                            }
                        }
                    }
                };

                channelToil.AddFinishAction(delegate
                {
                    foreach (var comp in effectComps)
                    {
                        if (comp is IChannelableAbility channelable)
                        {
                            channelable.IsChanneling = false;
                            channelable.OnChanneEnd(job.targetA);
                        }
                    }
                });

                channelToil.FailOn(() =>
                {
                    return this.pawn.DeadOrDowned || !Job.targetA.IsValid;
                });

                yield return channelToil;
            }
        }
    }
}
