using Verse;

namespace EMF
{
    public abstract class HediffComp_OnDamageTakenEffect : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            EventManager.Instance.OnBeforeThingDamageTaken += Event_OnBeforeThingDamageTaken;
            EventManager.Instance.OnPawnDamageTaken += Event_OnDamageTaken;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            EventManager.Instance.OnBeforeThingDamageTaken -= Event_OnBeforeThingDamageTaken;
            EventManager.Instance.OnPawnDamageTaken -= Event_OnDamageTaken;
        }

        protected void Event_OnDamageTaken(Pawn arg1, DamageInfo arg2)
        {
            if (arg1 == this.parent.pawn)
            {
                OnDamageTaken(arg2);
            }
        }

        protected virtual void OnDamageTaken(DamageInfo arg2)
        {

        }

        protected bool Event_OnBeforeThingDamageTaken(Thing arg1, ref DamageInfo arg2)
        {
            if (arg1 != this.parent.pawn)
            {
                return false;
            }
            else
            {
                return OnBeforeThingDamageTaken(ref arg2);
            }
        }

        protected virtual bool OnBeforeThingDamageTaken(ref DamageInfo arg2)
        {
            return false;
        }
    }

}