using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace EMF
{
    public class CompProperties_CursedEquip : CompProperties
    {
        public EffecterDef onLockEffect;
        public EffecterDef onUnlockEffect;

        public CompProperties_CursedEquip()
        {
            compClass = typeof(Comp_CursedEquipment);
        }
    }

    public class Comp_CursedEquipment : ThingComp
    {
        private bool hasBeenDispelled = false;
        private bool isSlotLocked = false;
        private Pawn cachedOwner = null;

        public CompProperties_CursedEquip Props => (CompProperties_CursedEquip)props;

        public bool IsSlotLocked => isSlotLocked;

        public void LockSlot()
        {
            Pawn owner = FindOwner();
            if (owner == null)
                return;

            if (IsSlotLocked)
            {
                return;
            }

            if (hasBeenDispelled)
            {
                return;
            }

            isSlotLocked = true;
            Messages.Message($"{parent.Label} has been locked to {owner.Name.ToStringShort}'s slot and cannot be removed.",
                MessageTypeDefOf.NeutralEvent, false);


            if (this.parent is Apparel parentApparel)
            {
                owner.apparel.Lock(parentApparel);
            }

            if (Props.onLockEffect != null)
            {
                Props.onLockEffect.Spawn(owner.Position, owner.Map);
            }
        }

        public void UnlockSlot()
        {
            Pawn owner = FindOwner();
            if (owner == null)
                return;

            if (!IsSlotLocked)
            {
                return;
            }

            isSlotLocked = false;
            Messages.Message($"{parent.Label} has been unlocked from {owner.Name.ToStringShort}'s slot and can now be removed.",
                MessageTypeDefOf.NeutralEvent, false);

            if (Props.onUnlockEffect != null)
            {
                Props.onUnlockEffect.Spawn(owner.Position, owner.Map);
            }
        }

        public void Dispell()
        {
            UnlockSlot();
            hasBeenDispelled = true;
        }

        private Pawn FindOwner()
        {
            if (cachedOwner != null && (parent.ParentHolder == cachedOwner.equipment || parent.ParentHolder == cachedOwner.apparel))
            {
                return cachedOwner;
            }

            if (parent.ParentHolder is Pawn_EquipmentTracker equipment)
            {
                cachedOwner = equipment.pawn;
                return cachedOwner;
            }

            if (parent.ParentHolder is Pawn_ApparelTracker apparel)
            {
                cachedOwner = apparel.pawn;
                return cachedOwner;
            }

            cachedOwner = null;
            return null;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            cachedOwner = pawn;
            LockSlot();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            isSlotLocked = false;
            cachedOwner = null;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            isSlotLocked = false;
            cachedOwner = null;
            base.PostDestroy(mode, previousMap);
        }


        public override string CompInspectStringExtra()
        {
            var info = new StringBuilder();

            if (isSlotLocked)
            {
                info.AppendLine("Locked: This item cannot be removed from its slot.");
            }

            return info.ToString().TrimEndNewlines();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isSlotLocked, "isSlotLocked", false);
            Scribe_Values.Look(ref hasBeenDispelled, "hasBeenDispelled", false);
            Scribe_References.Look(ref cachedOwner, "cachedOwner");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (FindOwner() != null)
            {
                if (isSlotLocked)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Unlock",
                        defaultDesc = "Unlock this item from its slot.",
                        icon = TexCommand.ForbidOff,
                        action = delegate
                        {
                            UnlockSlot();
                        }
                    };
                }
            }
        }
    }

}
