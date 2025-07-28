using RimWorld;
using Verse;

namespace EMF
{
    public static class PawnUtility_Duplicator
    {
        public static Pawn DuplicateDeadPawn(Pawn pawn, Faction heavenlyFaction)
        {
            if (pawn == null)
                return null;

            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
            float chronologicalYears = pawn.ageTracker.AgeChronologicalYearsFloat;
            if (chronologicalYears > ageBiologicalYearsFloat)
            {
                chronologicalYears = ageBiologicalYearsFloat;
            }

            PawnGenerationRequest request = new PawnGenerationRequest(
                   kind: pawn.kindDef,
                   faction: heavenlyFaction,
                   context: PawnGenerationContext.NonPlayer,
                   tile: -1,
                   forceGenerateNewPawn: true,
                   allowDead: false,
                   allowDowned: false,
                   canGeneratePawnRelations: false,
                   mustBeCapableOfViolence: true,
                   colonistRelationChanceFactor: 0f,
                   forceAddFreeWarmLayerIfNeeded: false,
                   allowGay: true,
                   allowPregnant: true,
                   allowFood: true,
                   allowAddictions: false,
                   inhabitant: false,
                   certainlyBeenInCryptosleep: false,
                   forceRedressWorldPawnIfFormerColonist: false,
                   worldPawnFactionDoesntMatter: false,
                   biocodeWeaponChance: 0f,
                   biocodeApparelChance: 0f,
                   fixedBiologicalAge: ageBiologicalYearsFloat,
                   fixedChronologicalAge: chronologicalYears,
                   fixedGender: pawn.gender
               );

            Pawn angelicPawn = PawnGenerator.GeneratePawn(request);

            CopyStoryAndTraits(pawn, angelicPawn);
            CopyApperance(pawn, angelicPawn);
            CopySkills(pawn, angelicPawn);
            ClearAndCopyApparel(pawn, angelicPawn);
            ClearAndCopyEquipment(pawn, angelicPawn);
            CopyGenes(pawn, angelicPawn);
            angelicPawn.Drawer.renderer.SetAllGraphicsDirty();
            return angelicPawn;
        }


        public static void CopyGenes(Pawn source, Pawn target)
        {
            if (source.genes == null || target.genes == null)
            {
                return;
            }


            target.genes.ClearXenogenes();

            foreach (var item in source.genes.Xenogenes)
            {
                target.genes.AddGene(item.def, true);
            }
        }

        public static void ClearAndCopyEquipment(Pawn source, Pawn target)
        {
            if (source.equipment == null || target.equipment == null)
            {
                return;
            }

            target.equipment.DestroyAllEquipment();
            foreach (var item in source.equipment.AllEquipmentListForReading)
            {
                ThingWithComps equipment = (ThingWithComps)ThingMaker.MakeThing(item.def, item.Stuff);
                target.equipment.AddEquipment(equipment);
            }
        }
        public static void ClearAndCopyApparel(Pawn source, Pawn target)
        {
            if (source.apparel == null || target.apparel == null)
            {
                return;
            }

            target.apparel.DestroyAll();
            foreach (var item in source.apparel.WornApparel)
            {
                Apparel equipment = (Apparel)ThingMaker.MakeThing(item.def, item.Stuff);
                target.apparel.Wear(equipment);
            }
        }
        public static void CopyStoryAndTraits(Pawn source, Pawn target)
        {
            if (source.story == null || target.story == null)
                return;

            // Copy childhood and adulthood
            target.story.Childhood = source.story.Childhood;
            target.story.Adulthood = source.story.Adulthood;

            // Copy traits
            target.story.traits.allTraits.Clear();
            foreach (Trait trait in source.story.traits.allTraits)
            {
                target.story.traits.GainTrait(new Trait(trait.def, trait.Degree, false));
            }

            // Copy title
            target.story.title = source.story.title;

            // Copy hair color
            target.story.HairColor = source.story.HairColor;
        }

        public static void CopyApperance(Pawn source, Pawn target)
        {
            if (source.story == null || target.story == null)
                return;

            // Copy body type
            target.story.bodyType = source.story.bodyType;

            // Copy hair
            target.story.hairDef = source.story.hairDef;
            target.story.HairColor = source.story.HairColor;
        }

        public static void CopySkills(Pawn source, Pawn target)
        {
            if (source.skills == null || target.skills == null)
                return;

            foreach (SkillRecord sourceSkill in source.skills.skills)
            {
                SkillRecord targetSkill = target.skills.GetSkill(sourceSkill.def);
                if (targetSkill != null)
                {
                    targetSkill.Level = sourceSkill.Level;
                    targetSkill.passion = sourceSkill.passion;
                    targetSkill.xpSinceLastLevel = sourceSkill.xpSinceLastLevel;
                }
            }
        }
    }
}
