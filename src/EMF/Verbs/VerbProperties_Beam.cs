using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EMF
{
    public class JobDriver_ChannelVerb : JobDriver
    {
        protected Verb_Channeled Verb_Channeled => (Verb_Channeled)this.pawn.jobs.curJob.verbToUse;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.B, false, 1f);
            yield return Toils_General.StopDead();

            Toil castToil = new Toil();
            castToil.initAction = delegate ()
            {
                if (!Verb_Channeled.TryStartCastOn(TargetA, TargetA, false, true, this.pawn.jobs.curJob.preventFriendlyFire, false))
                {
                    this.EndJobWith(JobCondition.Incompletable);
                }
                else
                {
                    Verb_Channeled.OnChannelStart(TargetA);
                }
            };
            castToil.handlingFacing = true;

            castToil.tickAction = () =>
            {
                if (Verb_Channeled != null)
                {
                    Verb_Channeled.OnChannelJobTick(TargetA);
                }

                castToil.actor.rotationTracker.FaceTarget(TargetA);
            };

            castToil.AddFinishAction(() =>
            {
                if (Verb_Channeled != null)
                {
                    Verb_Channeled.OnChannelEnd(TargetA);
                };
            });
            castToil.AddEndCondition(() =>
            {
                if (Verb_Channeled == null)
                {
                    return JobCondition.Incompletable;
                }

                if (!Verb_Channeled.CanChannel(TargetA))
                {
                    return JobCondition.Succeeded;
                }

                return JobCondition.Ongoing;
            });
            castToil.defaultCompleteMode = ToilCompleteMode.Never;
            //castToil.activeSkill = (() => Toils_Combat.GetActiveSkillForToil(castToil));

            castToil.FailOn((x) =>
            {
                return Verb_Channeled == null || !Verb_Channeled.CanChannel(TargetA);
            });

            yield return castToil;

            yield break;
        }



        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
    public class VerbProperties_Beam : VerbProperties_Channeled
    {
        public BeamParameters beamParms;

        public VerbProperties_Beam()
        {
            verbClass = typeof(Verb_LaserBeam);
        }
    }

    public class Verb_LaserBeam : Verb_Channeled
    {
        new protected VerbProperties_Beam Props => (VerbProperties_Beam)verbProps;

        private Beam_Thing currentBeam;

        //protected override bool TryCastShot()
        //{
        //    if (!this.currentTarget.HasThing)
        //        return false;
        //    return true;
        //}

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            return base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
        }


        public override void OnChannelStart(LocalTargetInfo target)
        {
            base.OnChannelStart(target);
            channelTicks = 0;
            currentBeam = Beam_Thing.Create(this.CasterPawn, target, null, Props.beamParms);
            GenSpawn.Spawn(currentBeam, this.CasterPawn.Position, this.CasterPawn.Map);
        }


        public override bool CanChannel(LocalTargetInfo target)
        {
            bool channelTicksValid = channelTicks < Props.channelDurationTicks;
            bool casterValid = CasterCanStillChannel();
            bool targetValid = IsTargetStillValid(target);

            if (!channelTicksValid)
                Log.Message("Beam ended: channelTicks (" + channelTicks + ") >= channelDurationTicks (" + Props.channelDurationTicks + ")");
            if (!casterValid)
                Log.Message("Beam ended: Caster cannot still channel");
            if (!targetValid)
                Log.Message("Beam ended: Target is no longer valid");

            return channelTicksValid && targetValid;
        }

        public override void OnChannelEnd(LocalTargetInfo target)
        {
            base.OnChannelEnd(target);
            EndBeam();
        }

        private bool CasterCanStillChannel()
        {
            if (CasterPawn == null)
                return false;

            return !CasterPawn.Downed &&
                   !CasterPawn.Dead &&
                   CasterPawn.Awake() &&
                   !CasterPawn.InMentalState &&
                   !CasterPawn.stances.stunner.Stunned;
        }

        private void ApplyDamageToTarget(Thing targetThing, float damageMultiplier)
        {
            float baseDamage = 10f;

            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.Burn,
                baseDamage * damageMultiplier,
                0.5f,
                -1,
                this.caster);

            targetThing.TakeDamage(dinfo);
        }

        private bool IsTargetStillValid(LocalTargetInfo target)
        {
            return target.HasThing &&
                   !target.Thing.Destroyed &&
                   target.Thing.Spawned;
        }


        private void EndBeam()
        {
            if (currentBeam != null)
            {
                currentBeam.Destroy();
                currentBeam = null;
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!target.HasThing)
            {
                if (showMessages)
                    Messages.Message("Cannot use beam: Must target an entity.", MessageTypeDefOf.RejectInput);
                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }

        public override void Reset()
        {
            EndBeam();
            channelTicks = 0;
            base.Reset();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref channelStartTick, "channelStartTick", -1);
        }
    }
}