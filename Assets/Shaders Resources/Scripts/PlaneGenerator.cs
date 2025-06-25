using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlaneGenerator : MonoBehaviour
{
    [Range(1, 250)] // Define the range in the editor
    public int resolution = 200; // Maximum Resolution of the plane

    public float size = 1f; // Overall size of the plane

    void Start()
    {
        GeneratePlane();
    }

    void GeneratePlane()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        // Clamp resolution to prevent excessive values
        resolution = Mathf.Clamp(resolution, 1, 250); // Adjusted range

        int numVertices = (resolution + 1) * (resolution + 1);
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];
        int[] triangles = new int[resolution * resolution * 6];

        float stepSize = size / resolution;
        float halfSize = size * 0.5f;

        for (int z = 0, i = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float u = (float)x / resolution;
                float v = (float)z / resolution;
                vertices[i] = new Vector3(u * size - halfSize, 0f, v * size - halfSize);
                uv[i] = new Vector2(u, v);

                if (x < resolution && z < resolution)
                {
                    int vertexIndex = x + z * (resolution + 1);
                    int triangleOffset = (x + z * resolution) * 6;
                    triangles[triangleOffset] = vertexIndex;
                    triangles[triangleOffset + 1] = vertexIndex + resolution + 1;
                    triangles[triangleOffset + 2] = vertexIndex + 1;
                    triangles[triangleOffset + 3] = vertexIndex + 1;
                    triangles[triangleOffset + 4] = vertexIndex + resolution + 1;
                    triangles[triangleOffset + 5] = vertexIndex + resolution + 2;
                }

                i++;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        GeneratePlane();
    }
#endif
}
