using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class TargetTracker : IExposable
    {
        private HashSet<Thing> trackedTargets = new HashSet<Thing>();

        public HashSet<Thing> Targets => trackedTargets.ToHashSet();

        public bool TryProcessTarget(Thing target, Action<Thing> onNewTargetFound)
        {
            if (trackedTargets.Add(target))
            {
                onNewTargetFound?.Invoke(target);
                return true;
            }

            return false;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref trackedTargets, "trackedTargets", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && trackedTargets == null)
            {
                trackedTargets = new HashSet<Thing>();
            }
        }

        public void Clear()
        {
            trackedTargets.Clear();
        }
    }
}
