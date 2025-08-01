using System.Collections.Generic;
using Verse;

namespace EMF
{
    public interface IDrawEquippedGizmos
    {
        IEnumerable<Gizmo> GetEquippedGizmos();
    }
}