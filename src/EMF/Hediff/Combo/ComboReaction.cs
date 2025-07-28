using System;
using System.Collections.Generic;
using Verse;

namespace EMF
{


    public class ComboReaction
    {
        public List<DamageDef> reactionDamageType;
        public List<HediffDef> reactionHediff;
        public string reactionDescription = "Dont leave me empty";
        public Type workerClass;
        public ComboReactionProperties reactionProperties;
        public bool removeOnReact = true;
        public EffecterDef reactionEffecter;



        private ComboReactionWorker worker = null;
        public ComboReactionWorker Worker
        {
            get
            {
                if (worker == null)
                {
                    worker = (ComboReactionWorker)Activator.CreateInstance(workerClass);
                    worker.Def = this;
                }

                return worker;
            }
        }


        public ComboReactionWorker ExecuteWorker(Pawn pawn, HediffComp_ComboReactor parent)
        {
            Worker.Def = this;
            Worker.Comp = parent;
            Worker.pawn = pawn;
            Worker.DoReaction(pawn);
            return worker;
        }
    }

    public class ComboReactionProperties
    {
        public DamageDef damageDef;
        public FloatRange damageRange;
        public FloatRange armourpenRange;

        public bool isAOE = false;
        public float radius = 5;

        public FriendlyFireSettings friendlyFireSetting = FriendlyFireSettings.All();
        public FloatRange severityRange;
    }
}