using MyMath.Primitives;
using Unity.Mathematics;
using UnityEngine;

namespace Generation
{
    public class MeshCreator
    {
        public Mesh CreateGridMesh(int rows, int columns, Edge edge, float width)
        {
            var vertices = CalculateVertices(rows, columns, width, edge, out var uvs, out var tangents);
            var triangles = CalculateTriangles(rows, columns);

            return CreateGridMesh(vertices, triangles, uvs, tangents);
        }

        public Mesh CreateGridMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector4[] tangents)
        {
            var mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                tangents = tangents,
                triangles = triangles
            };

            mesh.RecalculateNormals();

            return mesh;
        }

        public Vector3[] CalculateVertices(int rows, int columns, float width, Edge edge,
            out Vector2[] uvs, out Vector4[] tangents)
        {
            var start = edge.V1;
            var end = edge.V2;

            var depth = math.distance(start.x, end.x);
            var length = math.distance(start.z, end.z);

            if (depth > length)
            {
                start = new float3(start.xy, start.z - width / rows);
                end = new float3(end.xy, end.z - width / rows);
            }
            else if (depth < length)
            {
                start = new float3(start.x - width / rows, start.yz);
                end = new float3(end.x - width / rows, end.yz);
            }

            var vertices = new Vector3[(rows) * (columns + 1)];
            var percentage = (100f / columns) * 0.01f;
            var tangent = new Vector4(1f, 0f, 0f, -1f);
            uvs = new Vector2[vertices.Length];
            tangents = new Vector4[vertices.Length];

            var index = 0;
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns + 1; j++)
                {
                    // Get distance between start and end, based on j * percentage
                    // 0 = start | 0.5 = center | 1 = end
                    var lerp = math.lerp(start, end, j * percentage);

                    var x = lerp.x;
                    var y = lerp.y;
                    var z = lerp.z;

                    // Check on which side the path should be wide
                    var modifier = i * (width / rows);
                    if (depth > length)
                    {
                        z += modifier;
                    }
                    else
                    {
                        x += modifier;
                    }

                    var point = new Vector3(x, y, z);
                    vertices[index] = point;
                    uvs[index] = new Vector2(x / rows, z / columns);
                    tangents[index] = tangent;

                    index++;
                }
            }

            return vertices;
        }

        public int[] CalculateTriangles(int rows, int columns)
        {
            var triangles = new int[(((rows - 1) * (columns)) * 6) * 2];

            for (int i = 0, triIndex = 0, vertIndex = 0; i < rows - 1; i++, vertIndex++)
            {
                for (var j = 0; j < columns; j++, vertIndex++, triIndex += 12)
                {
                    // Front
                    triangles[triIndex] = vertIndex + columns + 1;
                    triangles[triIndex + 1] = vertIndex + columns + 2;
                    triangles[triIndex + 2] = vertIndex;

                    triangles[triIndex + 3] = vertIndex;
                    triangles[triIndex + 4] = vertIndex + columns + 2;
                    triangles[triIndex + 5] = vertIndex + 1;

                    // Back
                    triangles[triIndex + 6] = vertIndex + columns + 1;
                    triangles[triIndex + 7] = vertIndex;
                    triangles[triIndex + 8] = vertIndex + columns + 2;

                    triangles[triIndex + 9] = vertIndex;
                    triangles[triIndex + 10] = vertIndex + 1;
                    triangles[triIndex + 11] = vertIndex + columns + 2;
                }
            }

            return triangles;
        }
    }
}