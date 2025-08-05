using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_SpawnConalZoneEffect : CompProperties_AbilityZoneEffect
    {
        public int length = 10;
        public float angle = 90f;

        public CompProperties_SpawnConalZoneEffect()
        {
            compClass = typeof(CompAbilityEffect_SpawnConalZoneEffect);
        }
    }

    public class CompAbilityEffect_SpawnConalZoneEffect : AbilityZoneEffect
    {
        public CompProperties_SpawnConalZoneEffect Props => (CompProperties_SpawnConalZoneEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            ActiveZone zone = ActiveZone.SpawnZone(Props.zoneDef, this.parent.pawn.Position, TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, Props.length, Props.angle), this.parent.pawn.Map, this.parent.pawn);
        }


        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawFieldEdges(TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, Props.length, Props.angle), Color.cyan);
        }
    }

}