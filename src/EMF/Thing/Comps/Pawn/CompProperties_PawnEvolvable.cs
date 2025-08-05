using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EMF
{
    public class CompProperties_PawnEvolvable : CompProperties
    {
        public List<PawnKindDef> evolutionTargets = new List<PawnKindDef>();
        public List<Evolution> evolutionPaths = new List<Evolution>();
        public int cooldownTicks = 0;
        public EffecterDef transformEffect;
        public bool canDevolve = true;
        public EffecterDef devolveEffect;
        public ThingDef evolutionChamberDef;
        public bool useEvolutionPaths = false;

        public CompProperties_PawnEvolvable()
        {
            compClass = typeof(CompPawnEvolvable);
        }
    }

    public class CompPawnEvolvable : ThingComp, IThingHolder
    {
        public CompProperties_PawnEvolvable Props => (CompProperties_PawnEvolvable)props;
        private int lastEvolveTick = -999999;
        private bool evolutionReady = true;
        public ThingOwner<Thing> innerContainer;
        public Pawn storedPreviousForm;
        public Pawn storedNextForm;

        public bool HasPreviousForm => storedPreviousForm != null && innerContainer.Contains(storedPreviousForm);
        public bool HasNextForm => storedNextForm != null && innerContainer.Contains(storedNextForm);

        public bool CanEvolveNow
        {
            get
            {
                if (!evolutionReady)
                    return false;

                if (Props.cooldownTicks > 0 && Find.TickManager.TicksGame - lastEvolveTick < Props.cooldownTicks)
                    return false;

                Pawn pawn = parent as Pawn;
                if (pawn == null)
                    return false;

                if (pawn.Downed || pawn.Dead)
                    return false;

                if (pawn.InMentalState)
                    return false;

                return true;
            }
        }

        public bool CanDevolve => Props.canDevolve && innerContainer.Count > 0 && HasPreviousForm && !storedPreviousForm.Destroyed;

        public CompPawnEvolvable()
        {
            innerContainer = new ThingOwner<Thing>(this, true, LookMode.Deep, true);
        }

        public List<PawnKindDef> GetAvailableEvolutions()
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null) 
                return new List<PawnKindDef>();

            if (Props.useEvolutionPaths && Props.evolutionPaths != null)
            {
                var available = new List<PawnKindDef>();
                foreach (var path in Props.evolutionPaths)
                {
                    available.AddRange(path.GetAllAvailableEvolutions(pawn));
                }
                return available.Distinct().ToList();
            }
            else
            {
                return Props.evolutionTargets;
            }
        }

        public bool CanEvolveTo(PawnKindDef targetKind, out string failureReason)
        {
            failureReason = null;
            Pawn pawn = parent as Pawn;
            if (pawn == null)
            {
                failureReason = "Invalid pawn";
                return false;
            }

            if (Prefs.DevMode && DebugSettings.godMode)
            {
                return true;
            }

            if (!CanEvolveNow)
            {
                failureReason = GetDisabledReason();
                return false;
            }

            if (Props.useEvolutionPaths && Props.evolutionPaths != null)
            {
                foreach (var path in Props.evolutionPaths)
                {
                    foreach (var condition in path.conditions)
                    {
                        if (condition.newKind == targetKind)
                        {
                            var worker = condition.CreateWorker();
                            if (worker.MeetsCriteria(pawn))
                                return true;
                            else
                            {
                                failureReason = worker.GetFailureReason(pawn);
                                return false;
                            }
                        }
                    }
                }
                failureReason = "Evolution not available";
                return false;
            }
            else
            {
                return Props.evolutionTargets.Contains(targetKind);
            }
        }

        public void StorePreviousForm(Pawn previousForm)
        {
            if (!innerContainer.Contains(previousForm))
            {
                storedPreviousForm = previousForm;

                if (previousForm.Spawned)
                {
                    previousForm.DeSpawn();
                }

                innerContainer.TryAddOrTransfer(previousForm);
            }
        }

        public void StoreNextForm(Pawn nextForm)
        {
            if (!innerContainer.Contains(nextForm))
            {
                storedNextForm = nextForm;
                if (nextForm.Spawned)
                {
                    nextForm.DeSpawn();
                }
                innerContainer.TryAddOrTransfer(nextForm);
            }
        }

        public void Evolve(PawnKindDef targetKind)
        {
            Pawn currentPawn = parent as Pawn;
            string failureReason = "";
            if (currentPawn == null || !CanEvolveTo(targetKind, out failureReason))
            {
                if (!string.IsNullOrEmpty(failureReason))
                    Messages.Message(failureReason, MessageTypeDefOf.RejectInput);
                return;
            }

            IntVec3 position = currentPawn.Position;
            Rot4 rotation = currentPawn.Rotation;
            Map map = currentPawn.Map;

            Pawn newForm;

            if (HasNextForm && !storedNextForm.Destroyed)
            {
                newForm = storedNextForm;
                innerContainer.Remove(newForm);
            }
            else
            {
                newForm = PawnGenerator.GeneratePawn(targetKind, currentPawn.Faction);
            }

            GenSpawn.Spawn(newForm, position, map, rotation);

            CompPawnEvolvable newComp = newForm.TryGetComp<CompPawnEvolvable>();
            if (newComp != null)
            {
                newComp.StorePreviousForm(currentPawn);
            }

            Props.transformEffect?.Spawn(position, map);
            Find.Selector.Select(newForm);
            lastEvolveTick = Find.TickManager.TicksGame;
        }

        public void EvolveViaChamber(PawnKindDef targetKind)
        {
            Pawn currentPawn = parent as Pawn;
            string failureReason = "";
            if (currentPawn == null || !CanEvolveTo(targetKind, out failureReason))
            {
                if (!string.IsNullOrEmpty(failureReason))
                    Messages.Message(failureReason, MessageTypeDefOf.RejectInput);
                return;
            }

            if (Props.evolutionChamberDef != null)
            {
                ThingWithComps thing = (ThingWithComps)ThingMaker.MakeThing(Props.evolutionChamberDef, null);
                GenSpawn.Spawn(thing, this.parent.Position, this.parent.Map);

                if (thing != null && thing.TryGetComp(out CompEvolutionChamber compEvolutionChamber))
                {
                    compEvolutionChamber.StartMetamorphosis(currentPawn, targetKind);
                }

                Find.Selector.Select(thing);
            }
        }

        public void Devolve()
        {
            if (!CanDevolve)
                return;

            Pawn currentPawn = parent as Pawn;
            if (currentPawn == null || !HasPreviousForm || storedPreviousForm.Destroyed)
                return;

            IntVec3 position = currentPawn.Position;
            Map map = currentPawn.Map;

            innerContainer.Remove(storedPreviousForm);
            GenSpawn.Spawn(storedPreviousForm, position, map);

            CompPawnEvolvable prevComp = storedPreviousForm.TryGetComp<CompPawnEvolvable>();
            if (prevComp != null)
            {
                prevComp.StoreNextForm(currentPawn);
                prevComp.lastEvolveTick = Find.TickManager.TicksGame;
            }

            Find.Selector.Select(storedPreviousForm);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                var availableEvolutions = GetAvailableEvolutions();

                foreach (PawnKindDef evolutionTarget in availableEvolutions)
                {
                    string failureReason;
                    bool canEvolve = CanEvolveTo(evolutionTarget, out failureReason);

                    yield return new Command_Action
                    {
                        defaultLabel = $"Evolve to {evolutionTarget.label}",
                        defaultDesc = canEvolve ? $"Evolve into {evolutionTarget.label}" : failureReason,
                        action = delegate
                        {
                            if (canEvolve)
                            {
                                EvolveViaChamber(evolutionTarget);
                            }
                            else
                            {
                                Messages.Message(failureReason, MessageTypeDefOf.RejectInput);
                            }
                        },
                        Disabled = !canEvolve,
                        disabledReason = failureReason
                    };
                }

                if (HasPreviousForm && CanDevolve)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Devolve",
                        defaultDesc = $"Revert to previous form",
                        action = delegate
                        {
                            Devolve();
                        }
                    };
                }
            }
        }

        private string GetDisabledReason()
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null)
                return "Invalid pawn";
            if (pawn.Downed)
                return "Pawn is downed";
            if (pawn.InMentalState)
                return "Pawn is in mental state";
            if (!evolutionReady)
                return "Evolution not ready";
            if (Props.cooldownTicks > 0 && Find.TickManager.TicksGame - lastEvolveTick < Props.cooldownTicks)
                return "On cooldown";
            return "";
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastEvolveTick, "lastEvolveTick", -999999);
            Scribe_Values.Look(ref evolutionReady, "evolutionReady", true);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref storedPreviousForm, "storedPreviousForm");
            Scribe_References.Look(ref storedNextForm, "storedNextForm");
        }
    }
}