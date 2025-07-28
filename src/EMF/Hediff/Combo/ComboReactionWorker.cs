using RimWorld;
using Verse;

namespace EMF
{
    public abstract class ComboReactionWorker
    {
        public ComboReaction Def;
        public HediffComp_ComboReactor Comp;
        public Pawn pawn;

        public virtual void DoReaction(Pawn Pawn)
        {
            if (this.Def.reactionEffecter != null)
            {
                this.Def.reactionEffecter.Spawn(Pawn.Position, Pawn.Map, 2f);
            }

            MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, $"{this.GetType()} Reaction triggered!", 3f);
        }


        public abstract string GetExplanation(Pawn Pawn);
    }
}