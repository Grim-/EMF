using UnityEngine;
using Verse;

namespace EMF
{
    public class TransformableForm
    {
        public ThingDef thingDef;
        public PawnKindDef pawnKindDef;

        public bool IsPawn => pawnKindDef != null;
        public bool IsThing => thingDef != null && pawnKindDef == null;

        public string Label => IsPawn ? pawnKindDef.LabelCap : thingDef?.LabelCap ?? "Unknown";
        public Texture2D Icon => IsPawn ? pawnKindDef.race.uiIcon : thingDef?.uiIcon;
    }
}