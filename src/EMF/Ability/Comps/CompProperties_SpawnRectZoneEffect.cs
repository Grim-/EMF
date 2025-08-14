using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_SpawnRectZoneEffect : CompProperties_AbilityZoneEffect
    {
        public int length = 10;
        public int width = 3;
        public CompProperties_SpawnRectZoneEffect()
        {
            compClass = typeof(CompAbilityEffect_SpawnRectZoneEffect);
        }
    }

    public class CompAbilityEffect_SpawnRectZoneEffect : AbilityZoneEffect
    {
        public CompProperties_SpawnRectZoneEffect Props => (CompProperties_SpawnRectZoneEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            ActiveZone zone = ActiveZone.SpawnZone(Props.zoneDef, target.Cell, TargetUtil.GetAllCellsInRect(this.parent.pawn.Position, target.Cell, Props.width, Props.length).ToList(), this.parent.pawn.Map, this.parent.pawn);
        }


        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawFieldEdges(TargetUtil.GetAllCellsInRect(this.parent.pawn.Position, target.Cell, Props.width, Props.length), Color.red);
        }
    }
}