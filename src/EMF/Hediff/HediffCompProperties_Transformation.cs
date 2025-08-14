using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class HediffCompProperties_Transformation : HediffCompProperties
    {
        public EffecterDef transformationEffecter;
        public EffecterDef revertEffecter;
        public List<ThingDef> apparelToWear;
        public ThingDef weaponToEquip;
        public List<HediffDef> hediffsToAdd;
        public List<AbilityDef> abilitiesToGain;
        public List<GeneDef> genesToAdd;
        public bool lockEquipment = true;

        public HediffCompProperties_Transformation()
        {
            compClass = typeof(HediffComp_Transformation);
        }
    }

    public class HediffComp_Transformation : HediffComp, IThingHolder
    {
        private bool isTransformed;
        private ThingOwner<Thing> storedEquipment;
        private List<Hediff> addedHediffs = new List<Hediff>();
        private List<Gene> addedGenes = new List<Gene>();
        private List<Ability> addedAbilities = new List<Ability>();
        private ThingWithComps transformationWeapon;
        private List<Apparel> transformationApparel = new List<Apparel>();
        private Thing transformationSource;

        public override string CompLabelInBracketsExtra => isTransformed ? "Transformed" : base.CompLabelInBracketsExtra;

        public override string CompTipStringExtra
        {
            get
            {
                if (isTransformed)
                {
                    return "Currently in a transformed state. Original equipment is safely stored.";
                }
                return "Ready to transform.";
            }
        }
        public IThingHolder ParentHolder => this.parent.pawn;

        public HediffCompProperties_Transformation Props => (HediffCompProperties_Transformation)props;

        public HediffComp_Transformation()
        {
            storedEquipment = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public void SetTransformationSource(Thing source)
        {
            transformationSource = source;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ActivateTransformation();
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (isTransformed)
            {
                DeactivateTransformation(false);
            }
        }

        private void ActivateTransformation()
        {
            if (isTransformed || Pawn == null)
                return;

            if (this.Props.transformationEffecter != null)
            {
                this.Props.transformationEffecter.Spawn(this.Pawn.Position, this.Pawn.Map);
            }
            else
            {
                EffecterDefOf.Shield_Break.Spawn(this.Pawn.Position, this.Pawn.Map);
            }

            isTransformed = true;

            storedEquipment.Clear();
            if (Pawn.equipment?.Primary != null)
            {
                StoreCurrentEquipment();
            }

            if (Pawn.apparel != null)
            {
                StoreCurrentApparel();
            }

            if (Props.hediffsToAdd != null)
            {
                ApplyHediffs();
            }

            if (Pawn.genes != null && Props.genesToAdd != null)
            {
                ApplyGenes();
            }

            if (Pawn.abilities != null && Props.abilitiesToGain != null)
            {
                GainAbilities();
            }

            if (Props.weaponToEquip != null)
            {
                EquipTransformationWeapon();
            }

            if (Props.apparelToWear != null)
            {
                EquipTransformationApparel();
            }
        }

        private void StoreCurrentEquipment()
        {
            List<ThingWithComps> toStore = new List<ThingWithComps>();
            foreach (var thing in Pawn.equipment.GetDirectlyHeldThings())
            {
                if (thing != transformationSource && thing is ThingWithComps thingWithComps)
                {
                    toStore.Add(thingWithComps);
                }
            }

            foreach (var equiment in toStore)
            {
                Pawn.equipment.Remove(equiment);
                storedEquipment.TryAddOrTransfer(equiment, false);
            }
        }
        private void RestoreOriginalEquipmentAndApparel()
        {
            foreach (Thing item in storedEquipment.InnerListForReading.ToArray())
            {
                storedEquipment.Remove(item);

                if (item is Apparel apparel)
                {
                    Pawn.apparel.Wear(apparel, false);
                }
                else if (item is ThingWithComps equipment)
                {
                    Pawn.equipment.AddEquipment(equipment);
                }
            }
        }

        private void StoreCurrentApparel()
        {
            List<Apparel> toStore = new List<Apparel>();
            foreach (var thing in Pawn.apparel.GetDirectlyHeldThings())
            {
                if (thing != transformationSource && thing is Apparel apparel)
                {
                    toStore.Add(apparel);
                }
            }

            foreach (var apparel in toStore)
            {
                Pawn.apparel.Remove(apparel);
                storedEquipment.TryAddOrTransfer(apparel, false);
            }
        }

        private void EquipTransformationApparel()
        {
            foreach (var apparelDef in Props.apparelToWear)
            {
                var apparel = (Apparel)ThingMaker.MakeThing(apparelDef);
                Pawn.apparel.Wear(apparel, false, Props.lockEquipment);
                transformationApparel.Add(apparel);
            }
        }

        private void EquipTransformationWeapon()
        {
            transformationWeapon = (ThingWithComps)ThingMaker.MakeThing(Props.weaponToEquip);
            Pawn.equipment.MakeRoomFor(transformationWeapon);
            Pawn.equipment.AddEquipment(transformationWeapon);
        }

        private void GainAbilities()
        {
            foreach (var abilityDef in Props.abilitiesToGain)
            {
                Pawn.abilities.GainAbility(abilityDef);
                addedAbilities.Add(Pawn.abilities.GetAbility(abilityDef));
            }
        }

        private void ApplyGenes()
        {
            foreach (var geneDef in Props.genesToAdd)
            {
                var gene = Pawn.genes.AddGene(geneDef, false);
                addedGenes.Add(gene);
            }
        }

        private void ApplyHediffs()
        {
            foreach (var hediffDef in Props.hediffsToAdd)
            {
                var hediff = HediffMaker.MakeHediff(hediffDef, Pawn);
                addedHediffs.Add(hediff);
                Pawn.health.AddHediff(hediff);
            }
        }

        private void DeactivateTransformation(bool dropAllEquipment = false)
        {
            if (!isTransformed || Pawn == null)
                return;

            if (this.Props.revertEffecter != null)
            {
                this.Props.revertEffecter.Spawn(this.Pawn.Position, this.Pawn.Map);
            }
            else EffecterDefOf.Shield_Break.Spawn(this.Pawn.Position, this.Pawn.Map);

            if (transformationWeapon != null && Pawn.equipment?.Contains(transformationWeapon) == true)
            {
                Pawn.equipment.Remove(transformationWeapon);
            }
            if (!transformationWeapon?.Destroyed ?? false)
            {
                transformationWeapon.Destroy();
            }
            transformationWeapon = null;

            if (transformationApparel != null)
            {
                RemoveTransformationApparel();
            }

            foreach (var hediff in addedHediffs)
            {
                RemoveAddedHediffs(hediff);
            }
            addedHediffs.Clear();

            if (Pawn.genes != null)
            {
                RemoveGainedGenes();
            }
            addedGenes.Clear();

            if (Pawn.abilities != null)
            {
                RemoveGainedAbilities();
            }
            addedAbilities.Clear();

            if (dropAllEquipment)
            {
                storedEquipment.TryDropAll(Pawn.Position, Pawn.Map, ThingPlaceMode.Near);
            }
            else
            {
                RestoreOriginalEquipmentAndApparel();
            }

            storedEquipment.Clear();
            transformationSource = null;
            isTransformed = false;
        }


        private void RemoveGainedAbilities()
        {
            foreach (var ability in addedAbilities)
            {
                Pawn.abilities.RemoveAbility(ability.def);
            }
        }
        private void RemoveGainedGenes()
        {
            foreach (var gene in addedGenes)
            {
                Pawn.genes.RemoveGene(gene);
            }
        }
        private void RemoveAddedHediffs(Hediff hediff)
        {
            if (Pawn.health?.hediffSet.HasHediff(hediff.def) == true)
            {
                Pawn.health.RemoveHediff(hediff);
            }
        }
        private void RemoveTransformationApparel()
        {
            foreach (var apparel in transformationApparel)
            {
                if (Pawn.apparel?.WornApparel.Contains(apparel) == true)
                {
                    Pawn.apparel.Remove(apparel);
                }
                if (!apparel.Destroyed)
                {
                    apparel.Destroy();
                }
            }
            transformationApparel.Clear();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return storedEquipment;
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isTransformed, "isTransformed", false);
            Scribe_Deep.Look(ref storedEquipment, "storedEquipment", this);
            Scribe_Collections.Look(ref addedHediffs, "addedHediffs", LookMode.Reference);
            Scribe_Collections.Look(ref addedGenes, "addedGenes", LookMode.Reference);
            Scribe_Collections.Look(ref addedAbilities, "addedAbilities", LookMode.Reference);
            Scribe_References.Look(ref transformationWeapon, "transformationWeapon");
            Scribe_Collections.Look(ref transformationApparel, "transformationApparel", LookMode.Reference);
            Scribe_References.Look(ref transformationSource, "transformationSource");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (addedHediffs == null)
                {
                    addedHediffs = new List<Hediff>();
                }
                if (addedGenes == null)
                {
                    addedGenes = new List<Gene>();
                }

                if (addedAbilities == null)
                {
                    addedAbilities = new List<Ability>();
                }

                if (transformationApparel == null)
                {
                    transformationApparel = new List<Apparel>();
                }
            }
        }
    }
}