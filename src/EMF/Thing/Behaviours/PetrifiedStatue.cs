using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    [StaticConstructorOnStartup]
    public class PetrifiedStatue : ThingWithComps, IThingHolder
    {
        private ThingOwner<Thing> innerContainer;
        private Faction restoreFaction = null;

        public event Action<IntVec3, Thing> OnUnpetrify;
        private Color overlayColor = new Color(1, 1, 1, 0.7f);
        private Rot4 facingDirection = Rot4.South;
        protected Pawn PetrifiedThing
        {
            get
            {
                if (this.innerContainer.InnerListForReading.Count <= 0)
                {
                    return null;
                }
                return this.innerContainer.InnerListForReading[0] as Pawn;
            }
        }

        public PetrifiedStatue()
        {
            this.innerContainer = new ThingOwner<Thing>(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                if (PetrifiedThing == null || !(PetrifiedThing is Pawn))
                {
                    GenerateRandomPetrifiedPawn();
                }
            }
        }


        private void GenerateRandomPetrifiedPawn()
        {
            PawnKindDef kindDef = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(k => k.RaceProps.Humanlike).RandomElement();

            if (kindDef != null)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    kind: kindDef,
                    faction: null,
                    context: PawnGenerationContext.NonPlayer));

                if (pawn != null)
                {
                    if (this.innerContainer.TryAdd(pawn, true))
                    {
                        if (pawn.RaceProps.Humanlike)
                        {
                            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
                            PawnApparelGenerator.GenerateStartingApparelFor(pawn, new PawnGenerationRequest(kindDef, null, PawnGenerationContext.NonPlayer));
                        }
                    }
                }
            }
        }


        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            if (PetrifiedThing == null || !(PetrifiedThing is Pawn pawn))
                return;

            pawn.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc, facingDirection, false);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.PetrifiedThing != null)
            {
                Pawn pawn = UnpetrifyThing(false);
                DealUnpetrificationDamage(pawn);
            }
            base.Destroy(mode);
        }

        public void DealUnpetrificationDamage(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            if (Rand.Bool)
            {
                HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, false);
            }
            else
            {
                HealthUtility.DamageLimbsUntilIncapableOfManipulation(pawn, false);
            }
        }


        public Pawn UnpetrifyThing(bool destroy = true)
        {
            Thing petrifiedThing = this.PetrifiedThing;
            if (petrifiedThing == null)
            {
                return null;
            }

            if (restoreFaction != null)
            {
                petrifiedThing.SetFaction(restoreFaction);
            }

            Map map = this.Map;
            if (map == null)
            {
                return null;
            }

            if (petrifiedThing is Pawn pawn)
            {
                pawn.story.skinColorOverride = null;
            }


            IntVec3 position = this.Position;
            OnUnpetrify?.Invoke(position, petrifiedThing);

            if (this.innerContainer.TryDrop(petrifiedThing, position, map, ThingPlaceMode.Near, out Thing thing, null, null, false))
            {
                if (destroy)
                {
                    this.Destroy(DestroyMode.Vanish);
                }

                return thing as Pawn;
            }

            return null;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
        public static PetrifiedStatue PetrifyPawn(ThingDef statueDef, Pawn pawn, IntVec3 position, Map map)
        {
            PetrifiedStatue statue = (PetrifiedStatue)ThingMaker.MakeThing(statueDef, null);

            if (statue == null)
            {
                return null;
            }

            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }


            pawn.story.skinColorOverride = Color.gray;

            statue.restoreFaction = pawn.Faction;
            pawn.SetFaction(null);

            if (!statue.innerContainer.TryAdd(pawn, true))
            {
                Log.Error("Failed to add pawn to petrified statue container: " + pawn);
                return null;
            }

            GenSpawn.Spawn(statue, position, map);
            return statue;
        }

        public static PetrifiedStatue PetrifyPawn(Pawn pawn, IntVec3 position, Map map)
        {
            return PetrifyPawn(EMFDefOf.EMF_PetrifiedStatue, pawn, position, map);
        }

        public override string GetInspectString()
        {
            return PetrifiedThing != null ? $"Eerie Statue of a {PetrifiedThing.def.LabelCap}." : base.GetInspectString();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref this.innerContainer, "innerContainer", this);
            Scribe_References.Look(ref restoreFaction, "restoreFaction");
        }
    }
}
