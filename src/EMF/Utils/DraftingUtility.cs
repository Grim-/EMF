using Verse;

namespace EMF
{
    public static class DraftingUtility
    {
        public static WorldComp_DraftingManager DraftManager
        {
            get
            {
                if (Current.Game != null && Current.Game.World != null)
                {
                    return Current.Game.World.GetComponent<WorldComp_DraftingManager>();
                }

                return null;
            }
        }

        public static void RegisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.RegisterDraftableCreature(pawn);
            }
        }

        public static void UnregisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.UnregisterDraftableCreature(pawn);
            }
        }

        public static bool IsDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                return DraftManager.IsDraftableCreature(pawn);
            }

            return false;
        }

        public static void MakeDraftable(this Pawn pawn)
        {
            RegisterDraftableCreature(pawn);
        }
    }
}
