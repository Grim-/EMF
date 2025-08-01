using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_EvolutionChamber : CompProperties
    {
        public int metamorphosisTicks = 6000;
        public EffecterDef cocooningEffect;
        public EffecterDef emergingEffect;
        public bool showProgressBar = true;

        public CompProperties_EvolutionChamber()
        {
            compClass = typeof(CompEvolutionChamber);
        }
    }

    public class CompEvolutionChamber : ThingComp, IThingHolder
    {
        public CompProperties_EvolutionChamber Props => (CompProperties_EvolutionChamber)props;

        private ThingOwner<Thing> innerContainer;
        private Pawn evolvingPawn;
        private PawnKindDef targetKind;
        private int metamorphosisStartTick = -1;
        private bool metamorphosisComplete = false;
        private Effecter currentEffecter;

        public bool CanProgress => evolvingPawn != null && targetKind != null;
        public bool IsMetamorphosing => evolvingPawn != null && metamorphosisStartTick >= 0;

        public float MetamorphosisProgress
        {
            get
            {
                if (!IsMetamorphosing)
                    return 0f;
                return (float)(Find.TickManager.TicksGame - metamorphosisStartTick) / Props.metamorphosisTicks;
            }
        }


        private Effecter ProgressBar;

        public CompEvolutionChamber()
        {
            innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep, true);
        }

        public void SetTargetKind(PawnKindDef targetDef)
        {
            targetKind = targetDef;
        }

        public void StartMetamorphosis(Pawn pawn, PawnKindDef targetDef)
        {
            evolvingPawn = pawn;
            targetKind = targetDef;
            StorePawn(pawn);
            metamorphosisStartTick = Find.TickManager.TicksGame;



            if (ProgressBar == null)
            {
                EffecterDef progressBar = EffecterDefOf.ProgressBar;
                ProgressBar = progressBar.Spawn();
            }

            if (Props.cocooningEffect != null)
            {
                currentEffecter = Props.cocooningEffect.Spawn();
                currentEffecter.Trigger(parent, parent);
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!IsMetamorphosing)
                return;

            currentEffecter?.EffectTick(parent, parent);

            if (Props.showProgressBar && ProgressBar != null)
            {
                ProgressBar.EffectTick(parent, TargetInfo.Invalid);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)ProgressBar.children[0]).mote;
                if (mote != null)
                {
                    mote.progress = MetamorphosisProgress;
                    mote.offsetZ = -0.5f;
                }
            }

            if (MetamorphosisProgress >= 1f && !metamorphosisComplete)
            {
                FinishEvolution();
            }
        }

        public void FinishEvolution()
        {
            if (metamorphosisComplete || evolvingPawn == null || targetKind == null)
                return;

            metamorphosisComplete = true;

            currentEffecter?.Cleanup();
            ProgressBar?.Cleanup();

            IntVec3 position = parent.Position;
            Rot4 rotation = parent.Rotation;
            Map map = parent.Map;

            Pawn newForm = PawnGenerator.GeneratePawn(targetKind, evolvingPawn.Faction);

            //if (evolvingPawn.Name != null)
            //    newForm.Name = evolvingPawn.Name;

            if (evolvingPawn.relations != null)
            {
                foreach (var rel in evolvingPawn.relations.DirectRelations)
                {
                    newForm.relations.AddDirectRelation(rel.def, rel.otherPawn);
                }
            }

            GenSpawn.Spawn(newForm, position, map, rotation);

            CompPawnEvolvable newComp = newForm.TryGetComp<CompPawnEvolvable>();
            if (newComp != null)
            {
                if (innerContainer.Contains(evolvingPawn))
                {
                    innerContainer.Remove(evolvingPawn);
                }
                newComp.StorePreviousForm(evolvingPawn);
            }
            else
            {
                evolvingPawn.Destroy();
            }

            if (Props.emergingEffect != null)
            {
                Props.emergingEffect.Spawn().Trigger(newForm, newForm);
            }

            parent.Destroy();
        }

        public void StorePawn(Pawn pawn)
        {
            if (pawn == null || innerContainer.Contains(pawn))
                return;

            evolvingPawn = pawn;
            pawn.DeSpawn();
            innerContainer.TryAddOrTransfer(pawn);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref evolvingPawn, "evolvingPawn");
            Scribe_Defs.Look(ref targetKind, "targetKind");
            Scribe_Values.Look(ref metamorphosisStartTick, "metamorphosisStartTick", -1);
            Scribe_Values.Look(ref metamorphosisComplete, "metamorphosisComplete", false);
        }
    }
}