using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class WorldComp_DraftingManager : WorldComponent
    {
        private HashSet<Pawn> draftableCreatures = new HashSet<Pawn>();

        public WorldComp_DraftingManager(World world) : base(world)
        {

        }

        public void RegisterDraftableCreature(Pawn pawn)
        {
            if (pawn != null)
            {
                if (!draftableCreatures.Contains(pawn))
                {
                    draftableCreatures.Add(pawn);
                }

                EnsureDraftComponents(pawn);
            }
        }

        public void UnregisterDraftableCreature(Pawn pawn)
        {
            if (pawn != null)
            {
                draftableCreatures.Remove(pawn);
            }
        }

        public bool IsDraftableCreature(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }
            return draftableCreatures.Contains(pawn);
        }

        private void EnsureDraftComponents(Pawn pawn)
        {
            if (pawn.equipment == null)
            {
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
            }

            if (pawn.drafter == null)
            {
                pawn.drafter = new Pawn_DraftController(pawn);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref draftableCreatures, "draftableCreatures", LookMode.Reference);


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (draftableCreatures != null)
                {
                    foreach (var item in draftableCreatures)
                    {
                        EnsureDraftComponents(item);
                    }
                }
            }
        }
    }
}
