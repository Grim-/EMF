using System.Collections.Generic;
using Verse;

namespace EMF
{
    public interface IDrawInventoryGizmos
    {
        IEnumerable<Gizmo> GetInventoryGizmos();
    }
}