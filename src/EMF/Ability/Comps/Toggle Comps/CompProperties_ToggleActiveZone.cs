using RimWorld;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_ToggleActiveZone : CompProperties_AbilityEffect
    {
        public ActiveZoneDef domainZoneDef;
        public float domainRadius = 10f;

        public CompProperties_ToggleActiveZone()
        {
            compClass = typeof(CompAbilityEffect_ToggleActiveZone);
        }
    }

    public class CompAbilityEffect_ToggleActiveZone : CompAbilityEffect_Toggleable
    {
        public new CompProperties_ToggleActiveZone Props => (CompProperties_ToggleActiveZone)props;

        protected ActiveZone CurrentZone = null;

        public override bool CanStart()
        {
            return true;
        }

        public override void OnToggleOff()
        {
            if (CurrentZone != null)
            {
                CurrentZone.Destroy();
                CurrentZone = null;
            }
        }

        public override void OnToggleOn()
        {
            CurrentZone = ActiveZone.SpawnZone(
                Props.domainZoneDef,
                this.parent.pawn.Position,
                GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.domainRadius, true).ToList(),
                parent.pawn.Map,
                parent.pawn
            );
        }
    }
}
