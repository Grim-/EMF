using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public abstract class CompProperties_BaseJumpEffect : CompProperties_AbilityEffect
    {
        public float landingRadius = 3f;
        public EffecterDef landingEffecterDef = null;
    }

    public class CompAbilityEffect_BaseJumpEffect : CompAbilityEffect
    {
        protected IntVec3 startcell;
        protected IntVec3 targetCell;
        protected ThingFlyer thingFlyer;
        protected bool pawnWasDrafted = false;
        protected bool pawnWasSelected = false;
        CompProperties_BaseJumpEffect Props => (CompProperties_BaseJumpEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (thingFlyer != null)
            {
                thingFlyer.OnRespawn -= OnLand;
                if (!thingFlyer.Destroyed)
                {
                    thingFlyer.Destroy();
                }
                thingFlyer = null;
            }

            Map map = this.parent.pawn.MapHeld;
            startcell = this.parent.pawn.Position;
            targetCell = target.Cell;
            pawnWasDrafted = this.parent.pawn.Drafted;
            pawnWasSelected = Find.Selector.IsSelected(this.parent.pawn);
            thingFlyer = CreateFlyer(startcell, targetCell, map);

        }

        protected virtual ThingFlyer CreateFlyer(IntVec3 startPosition, IntVec3 targetPosition, Map map)
        {
            ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(EMFDefOf.EMF_ThingFlyer, this.parent.pawn, targetPosition, map, null, null, this.parent.pawn, this.parent.pawn.DrawPos, false);
            thingFlyer = ThingFlyer.LaunchFlyer(thingFlyer, this.parent.pawn, startPosition, map);
            thingFlyer.OnRespawn += OnLand;
            return thingFlyer;
        }

        protected virtual void OnLand(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            if (Props.landingEffecterDef != null)
            {
                Props.landingEffecterDef.Spawn(arg1, this.parent.pawn.Map);
            }

            if (pawnWasDrafted)
            {
                this.parent.pawn.drafter.Drafted = true;
            }

            if (pawnWasSelected)
            {
                Find.Selector.Select(this.parent.pawn, false);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawRadiusRing(target.Cell, Props.landingRadius);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref pawnWasDrafted, "pawnWasDrafted");
            Scribe_Values.Look(ref pawnWasSelected, "pawnWasSelected");
        }
    }
}
