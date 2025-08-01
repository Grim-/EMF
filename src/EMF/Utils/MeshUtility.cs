using UnityEngine;

namespace EMF
{
    public static class MeshUtility
    {
        public static Mesh CreateVariableWidthBeamMesh(float baseWidth, float length, AnimationCurve widthCurve = null, int segments = 10)
        {
            if (widthCurve == null)
            {
                widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
            }

            int vertCount = (segments + 1) * 2;
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            float segmentLength = length / segments;
            float uvSegmentLength = (length / 2f) / segments;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float z = -length * 0.5f + length * t;
                float width = baseWidth * widthCurve.Evaluate(t);

                // Left vertex
                vertices[i * 2] = new Vector3(-width * 0.5f, 0f, z);
                uvs[i * 2] = new Vector2(0f, uvSegmentLength * i);

                // Right vertex
                vertices[i * 2 + 1] = new Vector3(width * 0.5f, 0f, z);
                uvs[i * 2 + 1] = new Vector2(1f, uvSegmentLength * i);
            }

            // Create triangles
            int triCount = segments * 6;
            int[] triangles = new int[triCount];

            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 6;
                int vertIndex = i * 2;

                // First triangle
                triangles[baseIndex] = vertIndex;
                triangles[baseIndex + 1] = vertIndex + 2;
                triangles[baseIndex + 2] = vertIndex + 3;

                // Second triangle
                triangles[baseIndex + 3] = vertIndex;
                triangles[baseIndex + 4] = vertIndex + 3;
                triangles[baseIndex + 5] = vertIndex + 1;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
        public static Mesh CreateTiledBeamMesh(float width, float length)
        {
            Vector3[] vertices = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            int[] triangles = new int[6];

            vertices[0] = new Vector3(-width * 0.5f, 0f, -length * 0.5f);
            vertices[1] = new Vector3(-width * 0.5f, 0f, length * 0.5f);
            vertices[2] = new Vector3(width * 0.5f, 0f, length * 0.5f);
            vertices[3] = new Vector3(width * 0.5f, 0f, -length * 0.5f);

            float tileLength = length / 2f;
            uvs[0] = new Vector2(0f, 0f);
            uvs[1] = new Vector2(0f, tileLength);
            uvs[2] = new Vector2(1f, tileLength);
            uvs[3] = new Vector2(1f, 0f);

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}