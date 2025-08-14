using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_TransformingTrainedCreature : CompProperties_TransformingThing
    {
        public List<TrainableDef> autoTrainSkills = new List<TrainableDef>();
        public bool fullyTrain = false;
        public bool revertOnDeath = true;
        public bool revertOnDowned = true;

        public CompProperties_TransformingTrainedCreature()
        {
            compClass = typeof(CompTransformingTrainedCreature);
        }
    }


    public class CompTransformingTrainedCreature : CompTransformingThing
    {
        public new CompProperties_TransformingTrainedCreature Props =>
            (CompProperties_TransformingTrainedCreature)props;

        protected override void PostFormSpawned(ThingWithComps form, Pawn equipper)
        {
            base.PostFormSpawned(form, equipper);
            if (form is Pawn pawn && pawn.training != null)
            {
                ApplyTraining(pawn);
            }
        }


        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);

            if (Props.revertOnDeath && this.CanRevert)
            {
                this.Revert();
            }
        }

        public override void Notify_Downed()
        {
            base.Notify_Downed();

            if (Props.revertOnDowned && this.CanRevert)
            {
                this.Revert();
            }
        }

        private void Train(Pawn pawn, Pawn trainer, TrainableDef trainableDef)
        {
            if (pawn.training.CanBeTrained(trainableDef))
            {
                pawn.training.Train(trainableDef, trainer, complete: true);
                pawn.training.SetWantedRecursive(trainableDef, true);
            }
        }

        private void ApplyTraining(Pawn pawn)
        {
            var trainer =  GetEquipper();

            if (trainer == null || pawn.training == null)
            {
                Log.Message("CompTransformingTrainedCreature TRAINER OR TRAINING NULL");
                return;
            }
                

            Log.Message("CompTransformingTrainedCreature ApplyTraining");

            if (Props.fullyTrain)
            {
                foreach (var trainable in DefDatabase<TrainableDef>.AllDefs)
                {
                    Train(pawn, trainer, trainable);
                }
            }
            else
            {
                foreach (var trainable in Props.autoTrainSkills)
                {
                    Train(pawn, trainer, trainable);
                }
            }

            pawn.playerSettings.Master = trainer;
            pawn.playerSettings.followDrafted = true;
            pawn.playerSettings.followFieldwork = true;
        }
    }
}