using System.IO;
using UnityEditor;
using UnityEngine;

namespace VfxSandbox.Editor
{
    public class ProceduralMeshGenerator : EditorWindow
    {
        [MenuItem("Window/VFX/Generate Procedural Meshes")]
        public static void Generate()
        {
            string dir = "Assets/VfxSandbox/Meshes";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            GenerateFunnelMesh(dir + "/vfx_mesh_funnel_01.asset");
            GenerateRockMesh(dir + "/vfx_mesh_rock_01.asset");
            GenerateConeMesh(dir + "/vfx_mesh_cone_01.asset");

            AssetDatabase.Refresh();
            Debug.Log("✓ Procedural meshes generated successfully in Assets/VfxSandbox/Meshes");
        }

        private static void GenerateFunnelMesh(string path)
        {
            Mesh mesh = new Mesh();
            mesh.name = "vfx_mesh_funnel_01";

            int segments = 24;
            int rows = 12;
            float bottomRadius = 0.5f;
            float topRadius = 3f;
            float height = 6f;

            int vertCount = (segments + 1) * rows;
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] triangles = new int[segments * (rows - 1) * 6];

            for (int r = 0; r < rows; r++)
            {
                float t = (float)r / (rows - 1);
                float radius = Mathf.Lerp(bottomRadius, topRadius, t);
                float y = t * height;

                for (int s = 0; s <= segments; s++)
                {
                    float angle = (float)s / segments * Mathf.PI * 2;
                    float x = Mathf.Cos(angle) * radius;
                    float z = Mathf.Sin(angle) * radius;

                    int idx = r * (segments + 1) + s;
                    
                    // Twist the vertices around Y axis based on height to create helical mesh structure
                    float twist = t * 1.5f;
                    float tx = x * Mathf.Cos(twist) - z * Mathf.Sin(twist);
                    float tz = x * Mathf.Sin(twist) + z * Mathf.Cos(twist);

                    vertices[idx] = new Vector3(tx, y, tz);
                    uvs[idx] = new Vector2((float)s / segments, t);
                }
            }

            int triIdx = 0;
            for (int r = 0; r < rows - 1; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int currRowIdx = r * (segments + 1);
                    int nextRowIdx = (r + 1) * (segments + 1);

                    triangles[triIdx++] = currRowIdx + s;
                    triangles[triIdx++] = nextRowIdx + s;
                    triangles[triIdx++] = currRowIdx + s + 1;

                    triangles[triIdx++] = currRowIdx + s + 1;
                    triangles[triIdx++] = nextRowIdx + s;
                    triangles[triIdx++] = nextRowIdx + s + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, path);
        }

        private static void GenerateRockMesh(string path)
        {
            Mesh mesh = new Mesh();
            mesh.name = "vfx_mesh_rock_01";

            int segments = 12;
            int rings = 8;
            float radius = 1.0f;

            int vertCount = (segments + 1) * (rings + 1);
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] triangles = new int[segments * rings * 6];

            Random.InitState(42); // deterministic seed for reproducibility

            for (int ring = 0; ring <= rings; ring++)
            {
                float phi = (float)ring / rings * Mathf.PI;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                for (int seg = 0; seg <= segments; seg++)
                {
                    float theta = (float)seg / segments * Mathf.PI * 2;
                    float sinTheta = Mathf.Sin(theta);
                    float cosTheta = Mathf.Cos(theta);

                    int idx = ring * (segments + 1) + seg;

                    Vector3 pos = new Vector3(
                        cosTheta * sinPhi,
                        cosPhi,
                        sinTheta * sinPhi
                    );

                    // Add rugged perturbation to vertices to make it look jagged
                    float noise = Mathf.PerlinNoise(pos.x * 3f + 10, pos.y * 3f + 20) * 0.4f;
                    pos *= (radius - 0.2f + noise);

                    vertices[idx] = pos;
                    uvs[idx] = new Vector2((float)seg / segments, (float)ring / rings);
                }
            }

            int triIdx = 0;
            for (int ring = 0; ring < rings; ring++)
            {
                for (int seg = 0; seg < segments; seg++)
                {
                    int currRingIdx = ring * (segments + 1);
                    int nextRingIdx = (ring + 1) * (segments + 1);

                    triangles[triIdx++] = currRingIdx + seg;
                    triangles[triIdx++] = nextRingIdx + seg;
                    triangles[triIdx++] = currRingIdx + seg + 1;

                    triangles[triIdx++] = currRingIdx + seg + 1;
                    triangles[triIdx++] = nextRingIdx + seg;
                    triangles[triIdx++] = nextRingIdx + seg + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, path);
        }

        private static void GenerateConeMesh(string path)
        {
            Mesh mesh = new Mesh();
            mesh.name = "vfx_mesh_cone_01";

            int segments = 24;
            float radius = 1.0f;
            float height = 1.5f;

            // Vertices: bottom circle (segments + 1), tip (1), center of bottom cap (1)
            int vertCount = (segments + 1) + 2;
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] triangles = new int[segments * 6];

            int tipIdx = vertCount - 2;
            int centerIdx = vertCount - 1;

            // Bottom circle vertices
            for (int s = 0; s <= segments; s++)
            {
                float angle = (float)s / segments * Mathf.PI * 2;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                vertices[s] = new Vector3(x, 0f, z);
                uvs[s] = new Vector2((float)s / segments, 0f);
            }

            // Tip vertex
            vertices[tipIdx] = new Vector3(0f, height, 0f);
            uvs[tipIdx] = new Vector2(0.5f, 1.0f);

            // Center of bottom circle
            vertices[centerIdx] = new Vector3(0f, 0f, 0f);
            uvs[centerIdx] = new Vector2(0.5f, 0f);

            int triIdx = 0;
            for (int s = 0; s < segments; s++)
            {
                // Sides triangles
                triangles[triIdx++] = s;
                triangles[triIdx++] = tipIdx;
                triangles[triIdx++] = s + 1;

                // Base triangles (facing down)
                triangles[triIdx++] = s + 1;
                triangles[triIdx++] = centerIdx;
                triangles[triIdx++] = s;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, path);
        }
    }
}
