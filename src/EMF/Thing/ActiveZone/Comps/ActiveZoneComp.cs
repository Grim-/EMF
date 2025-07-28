using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public abstract class ActiveZoneComp : ThingComp
    {
        protected ActiveZone _ParentZone;
        public ActiveZone ParentZone
        {
            get
            {
                if (_ParentZone == null && this.parent is ActiveZone)
                {
                    _ParentZone = this.parent as ActiveZone;
                }

                return _ParentZone;
            }
        }

        public virtual void OnZoneSpawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {

        }

        public virtual void OnZoneDespawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {

        }

        public virtual void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {

        }
        public virtual void OnThingEnteredZone(ActiveZone zone, Thing thing) 
        { 
        
        }

        public virtual void OnThingLeftZone(ActiveZone zone, Thing thing)    
        { 

        }
        protected HashSet<Thing> GetCurrentThingsInZone(ref List<IntVec3> cells)
        {
            return TargetUtil.GetThingsInCells(cells, this.parent.Map);
        }
    }
}
