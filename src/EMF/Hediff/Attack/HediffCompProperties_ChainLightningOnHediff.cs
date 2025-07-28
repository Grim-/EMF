using RimWorld;
using Verse;

namespace EMF
{
    //public class HediffCompProperties_ChainLightningOnHediff : HediffCompProperties
    //{
    //    public IntRange damage;
    //    public DamageDef damageDef;

    //    public HediffDef hediffDef;

    //    public HediffCompProperties_ChainLightningOnHediff()
    //    {
    //        compClass = typeof(HediffComp_ChainLightningOnHedif);
    //    }
    //}

    //public class HediffComp_ChainLightningOnHedif : HediffComp_OnMeleeAttackEffect
    //{
    //    HediffCompProperties_ChainLightningOnHediff Props => (HediffCompProperties_ChainLightningOnHediff)props;
    //    private StaggeredChainLightning chainLightning;
    //    protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
    //    {
    //        base.OnMeleeAttack(MeleeAttackVerb, Target);

    //        if (Target.Pawn != null)
    //        {
    //            if (Target.Pawn.health.hediffSet.HasHediff(Props.hediffDef))
    //            {
    //                chainLightning = new StaggeredChainLightning(parent.pawn?.Map, parent.pawn, 300, 2, 5, Props.damage.RandomInRange, Props.damageDef, (Thing) =>
    //                {
    //                    return Thing != this.parent.pawn;
    //                }, 5, 50);


    //                chainLightning.StartChain(Target.Pawn);
    //            }
    //        }
    //    }

    //    public override void CompPostTick(ref float severityAdjustment)
    //    {
    //        base.CompPostTick(ref severityAdjustment);


    //        if (chainLightning != null)
    //        {
    //            chainLightning.Tick();

    //            if (chainLightning.IsFinished)
    //            {
    //                chainLightning = null;
    //            }
    //        }
    //    }
    //}
}