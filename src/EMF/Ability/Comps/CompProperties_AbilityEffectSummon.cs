using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{


    public class CompProperties_AbilityEffectSummon : CompProperties_AbilityEffect
    {
        public PawnKindDef pawnKind;
        public HediffDef customSummonHediff;
        public bool autoTrain = true;
        public bool enableDrafting = true;
        public bool storeSummon = true;

        public CompProperties_AbilityEffectSummon()
        {
            compClass = typeof(CompAbilityEffect_Summon);
        }
    }

    public class CompAbilityEffect_Summon : CompAbilityEffect, IThingHolder
    {
        public new CompProperties_AbilityEffectSummon Props => (CompProperties_AbilityEffectSummon)props;

        public IThingHolder ParentHolder => this.parent.pawn;

        protected PawnStorageTracker PawnStore = null;

        protected Pawn StoredSummon = null;
        protected Pawn CurrentSummon = null;

        public CompAbilityEffect_Summon()
        {
            PawnStore = new PawnStorageTracker(this);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (CurrentSummon == null)
            {
                if (PawnStore.Any)
                {
                    CurrentSummon = SummonUtil.SpawnSummonFor(
                        PawnStore.RemoveNextStored(),
                        parent.pawn,
                        parent.pawn.Map,
                        target.Cell,
                        Props.autoTrain,
                        Props.enableDrafting,
                        Props.customSummonHediff
                    );
                }
                else
                {
                    CurrentSummon = SummonUtil.SpawnSummonFor(
                        Props.pawnKind,
                        parent.pawn,
                        parent.pawn.Map,
                        target.Cell,
                        Props.autoTrain,
                        Props.enableDrafting,
                        Props.customSummonHediff
                    );
                }
            }
            else
            {
                if (CurrentSummon.Spawned)
                {
                    CurrentSummon.DeSpawn();
                    PawnStore.StorePawn(CurrentSummon);
                }

                CurrentSummon = null;
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            PawnStore.GetChildHolders(outChildren);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return PawnStore.GetDirectlyHeldThings();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Deep.Look(ref PawnStore, "pawnStore");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (PawnStore == null)
                {
                    PawnStore = new PawnStorageTracker(this);
                }
            }
        }
    }
}
