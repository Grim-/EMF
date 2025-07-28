using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{

    public class HediffCompProperties_BoundWeapon : HediffCompProperties
    {
        public HediffCompProperties_BoundWeapon()
        {
            compClass = typeof(HediffComp_BoundWeapon);
        }
    }

    public class HediffComp_BoundWeapon : HediffComp, IThingHolder
    {
        protected Thing BoundRef;
        private ThingOwner innerContainer;

        public Thing BoundThing => BoundRef;
        public bool HasBoundThing => BoundRef != null;
        public bool IsStored => innerContainer?.Contains(BoundRef) ?? false;


        public IThingHolder ParentHolder => this;

        public override void CompPostMake()
        {
            base.CompPostMake();
            innerContainer = new ThingOwner<Thing>(this, true, LookMode.Deep);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void SetBoundWeapon(Thing boundThing)
        {
            BoundRef = boundThing;
        }

        public void ClearBinding()
        {
            if (HasBoundThing)
            {
                if (innerContainer.Remove(BoundRef))
                {
                    BoundRef = null;
                }
            }
        }

        private void RemoveFromCurrentLocation()
        {
            // Is spawned in world
            if (BoundRef.Spawned)
            {
                BoundRef.DeSpawn();
                return;
            }

            // Is in equipment
            if (BoundThing is ThingWithComps withComps && Pawn.equipment?.Contains(withComps) == true)
            {
                Pawn.equipment.Remove(withComps);
                return;
            }

            // Is in inventory
            if (Pawn.inventory?.innerContainer.Contains(BoundRef) == true)
            {
                Pawn.inventory.innerContainer.Remove(BoundRef);
                return;
            }

            // Is in some other container
            if (BoundRef.holdingOwner != null)
            {
                BoundRef.holdingOwner.Remove(BoundRef);
            }
        }

        public bool StoreWeapon()
        {
            if (!HasBoundThing || IsStored || BoundRef.Destroyed)
                return false;

            RemoveFromCurrentLocation();

            return innerContainer.TryAdd(BoundRef);
        }
        public bool SummonWeapon()
        {
            if (!HasBoundThing || BoundRef.Destroyed)
                return false;

            if (IsStored)
            {
                innerContainer.Remove(BoundRef);
            }
            else
            {
                if (BoundRef.Spawned && BoundRef.Map == Pawn.Map)
                {
                    LaunchRecallProjectile();
                    return true;
                }

                RemoveFromCurrentLocation();
            }


            if (BoundRef is ThingWithComps weaponComps && Pawn.equipment != null)
            {
                if (BoundRef.Spawned)
                {
                    BoundRef.DeSpawn();
                }

                Pawn.equipment.DropAndEquip(weaponComps);
                return true;
            }

            IntVec3 dropPos = Pawn.Position;
            if (!dropPos.Walkable(Pawn.Map))
            {
                dropPos = GenAdj.CellsAdjacent8Way(Pawn).FirstOrDefault(c => c.Walkable(Pawn.Map));
            }

            if (dropPos.IsValid)
            {
                if (!BoundRef.Spawned)
                {
                    GenSpawn.Spawn(BoundRef, dropPos, Pawn.Map);
                }
                else
                {
                    GenPlace.TryPlaceThing(BoundRef, dropPos, Pawn.Map, ThingPlaceMode.Near);
                }
                return true;
            }

            return false;
        }

        private void LaunchRecallProjectile()
        {
            Projectile_Delegate projectile = (Projectile_Delegate)ThingMaker.MakeThing(EMFDefOf.EMF_DelegateFlyer);
            GenSpawn.Spawn(projectile, BoundRef.Position, Pawn.Map);
            projectile.Launch(Pawn, Pawn, Pawn, ProjectileHitFlags.IntendedTarget);
            BoundRef.DeSpawn();
            projectile.OnImpact = OnWeaponRecallImpact;
        }

        private void OnWeaponRecallImpact(Projectile_Delegate projectile, Thing hitThing, bool wasBlocked)
        {
            if (Pawn?.equipment == null)
                return;

            if (BoundRef is ThingWithComps weaponComps)
            {
                Pawn.equipment.DropAndEquip(weaponComps);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {

            if (HasBoundThing && !BoundRef.Destroyed)
            {
                if (!IsStored && Pawn.equipment.AllEquipmentListForReading.Any(x => x == BoundThing))
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Store Bound Item",
                        defaultDesc = "Store your bound item in a pocket dimension.",
                        icon = TexButton.Suspend,
                        action = delegate
                        {
                            StoreWeapon();
                        }
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Summon Bound Item",
                        defaultDesc = "Summon your bound item to you.",
                        icon = TexButton.Rename,
                        action = delegate
                        {
                            SummonWeapon();
                        }
                    };
                }
            }
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (HasBoundThing)
                {
                    return IsStored ? "Stored" : "Summoned";
                }
                return base.CompLabelInBracketsExtra;
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                if (HasBoundThing)
                {
                    string status = IsStored ? "stored in pocket dimension" : "summoned";
                    return $"Bound item ({BoundRef.LabelCap}) is currently {status}";
                }
                return base.CompTipStringExtra;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref BoundRef, "boundReference");
            Scribe_Deep.Look(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }

    }
}