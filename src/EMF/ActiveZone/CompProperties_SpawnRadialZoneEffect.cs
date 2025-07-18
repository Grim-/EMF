using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_SpawnRadialZoneEffect : CompProperties_AbilityZoneEffect
    {
        public float radius = 5;

        public CompProperties_SpawnRadialZoneEffect()
        {
            compClass = typeof(CompAbilityEffect_SpawnRadialZoneEffect);
        }
    }

    public class CompAbilityEffect_SpawnRadialZoneEffect : AbilityZoneEffect
    {
        public CompProperties_SpawnRadialZoneEffect Props => (CompProperties_SpawnRadialZoneEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            ActiveZone zone = ActiveZone.SpawnZone(Props.zoneDef, target.Cell, GenRadial.RadialCellsAround(target.Cell, Props.radius, true).ToList(), this.parent.pawn.Map);
        }


        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawRadiusRing(target.Cell, Props.radius, Color.cyan);
        }
    }
}