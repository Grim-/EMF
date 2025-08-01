using UnityEngine;
using Verse;

namespace EMF
{
    public class BeamRenderer
    {
        private Beam_Thing parent;
        private MaterialPropertyBlock MPB;

        public BeamRenderer(Beam_Thing beam)
        {
            this.parent = beam;
        }

        public void DrawBeamSegment(BeamSegment segment, BeamParameters parameters)
        {
            float distance = Vector3.Distance(segment.startPoint, segment.endPoint);
            Vector3 midpoint = Vector3.Lerp(segment.startPoint, segment.endPoint, 0.5f);
            Vector3 position = midpoint;
            position.y = AltitudeLayer.MoteOverhead.AltitudeFor();

            int beamWidth = parameters != null ? parameters.beamWidth : 1;

            Mesh mesh = null;
            if (parameters.tileBeam)
            {
                mesh = MeshUtility.CreateTiledBeamMesh(beamWidth, distance);
            }
            else
            {
                mesh = MeshPool.GridPlane(new Vector2(beamWidth * 4, distance));
            }

            var beamGraphic = parent.BeamMoteDef.Graphic;
            if (MPB == null)
            {
                MPB = new MaterialPropertyBlock();
            }

            MPB.SetFloat(ShaderPropertyIDs.AgeSecs, Find.TickManager.TicksGame / 60f);
            MPB.SetFloat(ShaderPropertyIDs.AgeSecsPausable, Find.TickManager.TicksGame);

            float angle = (segment.endPoint - segment.startPoint).AngleFlat();
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Graphics.DrawMesh(mesh, position, rotation, beamGraphic.MatAt(Rot4.South), 0, null, 0, MPB);
        }


        protected Effecter beamStartEffecter = null;

        public void DrawBeamStart(Vector3 origin, BeamParameters parameters)
        {
            if (parameters == null || parameters.beamStartGraphic == null)
            {
                return;
            }

            Vector3 position = origin;
            position.y = AltitudeLayer.Skyfaller.AltitudeFor();
            Mesh mesh = MeshPool.plane20;
            var beamGraphic = parameters.beamStartGraphic.Graphic;
            Vector2 drawSize = beamGraphic.drawSize;
            Vector3 scale = new Vector3(drawSize.x, 1f, drawSize.y);

            float angle = (parent.currentEndpoint - origin).AngleFlat();
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
            Graphics.DrawMesh(mesh, matrix, beamGraphic.MatAt(Rot4.South), 0);


            if (beamStartEffecter == null)
            {
                if (parameters.beamStartEffecter != null)
                {
                    beamStartEffecter = parameters.beamStartEffecter.Spawn(position.ToIntVec3(), this.parent.Map);
                }
            }

 
        }
        protected Effecter beamEndEffecter = null;

        public void DrawBeamEnd(Vector3 endpoint, BeamParameters parameters)
        {
            if (parameters == null || parameters.beamEndGraphic == null)
            {
                return;
            }

            Vector3 position = endpoint;
            position.y = AltitudeLayer.Skyfaller.AltitudeFor();
            Mesh mesh = MeshPool.plane20;
            var beamGraphic = parameters.beamEndGraphic.Graphic;

            Vector2 drawSize = beamGraphic.drawSize;
            Vector3 scale = new Vector3(drawSize.x, 1f, drawSize.y);
            float angle = (endpoint - parent.Origin).AngleFlat();
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);

            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

            Graphics.DrawMesh(mesh, matrix, beamGraphic.MatAt(Rot4.South), 0);


            if (beamEndEffecter == null)
            {
                if (parameters.beamEndEffecter != null)
                {
                    beamEndEffecter = parameters.beamEndEffecter.Spawn(position.ToIntVec3(), this.parent.Map);
                }
            }
        }
    }
}