using RimWorld;
using Verse;
using static HarmonyLib.Code;

namespace EMF
{
    public class CompProperties_AbilitySummonPawn : CompProperties_AbilityEffect
    {
        public PawnKindDef summonKind;

        public CompProperties_AbilitySummonPawn()
        {
            compClass = typeof(CompAbilityEffect_SummonPawn);
        }
    }

    public class CompAbilityEffect_SummonPawn : CompAbilityEffect
    {
        new CompProperties_AbilitySummonPawn Props => (CompProperties_AbilitySummonPawn)props;
        private Pawn SummonedPawn = null;


        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (SummonedPawn != null)
            {
                if (!SummonedPawn.Destroyed)
                {
                    SummonedPawn.Destroy(DestroyMode.KillFinalize);
                }

                EventManager.Instance.OnThingKilled -= EventManager_OnThingKilled;
                SummonedPawn = null;
            }
            else
            {
                SummonedPawn = PawnGenerator.GeneratePawn(Props.summonKind, parent.pawn.Faction);

                SummonedPawn = (Pawn)GenSpawn.Spawn(SummonedPawn, parent.pawn.Position, parent.pawn.Map);


                if (!SummonedPawn.RaceProps.Humanlike)
                {
                    EMFUtil.TrainPawn(SummonedPawn, parent.pawn);
                }

                if (SummonedPawn != null)
                {
                    EventManager.Instance.OnThingKilled += EventManager_OnThingKilled;
                }
            }
        }

        private void EventManager_OnThingKilled(Pawn arg1, DamageInfo arg2, Hediff arg3)
        {
            if (SummonedPawn != null && arg1 == SummonedPawn)
            {
                EventManager.Instance.OnThingKilled -= EventManager_OnThingKilled;
                if (!SummonedPawn.Destroyed)
                {
                    SummonedPawn.Destroy(DestroyMode.KillFinalize);
                }

                Log.Message($"Summon died, destroying");
                SummonedPawn = null;
            }
        }
    }
}
