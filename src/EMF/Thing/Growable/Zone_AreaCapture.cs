using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class Zone_AreaCapture : Zone
    {
        public IntVec2 OriginSize;
        public bool CaptureFloors = true;
        public bool CaptureTerrain = true;
        public bool CaptureThings = true;
        public override bool IsMultiselectable => true;
        protected override Color NextZoneColor => new Color(0.2f, 0.6f, 0.3f);


        private static Dictionary<Map, Zone_AreaCapture> instances = new Dictionary<Map, Zone_AreaCapture>();

        public Zone_AreaCapture()
        {

        }

        public Zone_AreaCapture(ZoneManager zoneManager, IntVec2 originBuildingSize) : base("AreaCapture".Translate(), zoneManager)
        {
            this.OriginSize = originBuildingSize;
            this.color = new Color(0, 1, 0, 0.3f);
        }

        public override void PostRegister()
        {
            base.PostRegister();
            instances[Map] = this;
        }

        public override void PostDeregister()
        {
            base.PostDeregister();
            if (instances.ContainsKey(Map) && instances[Map] == this)
            {
                instances.Remove(Map);
            }
        }

        public static Zone_AreaCapture GetOrCreateForMap(Map map, IntVec2 originSize)
        {
            if (instances.TryGetValue(map, out Zone_AreaCapture existingZone))
            {
                return existingZone;
            }

            Zone_AreaCapture newZone = new Zone_AreaCapture(map.zoneManager, originSize);
            map.zoneManager.RegisterZone(newZone);
            instances[map] = newZone;
            return newZone;
        }

        public override IEnumerable<InspectTabBase> GetInspectTabs()
        {
            IntVec3 center = CellRect.FromCellList(this.cells).CenterCell;
            CellRect rect = CellRect.CenteredOn(center, 1);
            GenDraw.DrawFieldEdges(rect.Cells.ToList(), Color.red);
            return base.GetInspectTabs();
        }

        //base implementation but allows placing over existing stuff.
        public override void AddCell(IntVec3 c)
        {
            if (this.cells.Contains(c))
            {
                Log.Error(string.Concat(new object[]
                {
                    "Adding cell to zone which already has it. c=",
                    c,
                    ", zone=",
                    this
                }));
                return;
            }

            this.cells.Add(c);
            this.zoneManager.AddZoneGridCell(this, c);
            this.Map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Zone);
        }


        public override void Delete(bool playSound)
        {
            if (instances.ContainsKey(Map))
            {
                instances.Remove(Map);
            }
            base.Delete(playSound);
        }


        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneExpand_AreaCapture>();
            yield break;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Open Structure Editor UI",
                defaultDesc = "Edit the current structure in the editor",
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_StructureEditor(this, StructureBuilder.Instance.GetStructure()));
                }
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref OriginSize, "OriginSize");
            Scribe_Values.Look(ref CaptureFloors, "CaptureFloors", true);
            Scribe_Values.Look(ref CaptureTerrain, "CaptureTerrain", true);
            Scribe_Values.Look(ref CaptureThings, "CaptureThings", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                instances[Map] = this;
            }
        }

    }
}