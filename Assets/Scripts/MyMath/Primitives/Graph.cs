using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace MyMath.Primitives
{
    public class Graph
    {
        public List<EdgeEndpoint> Endpoints;
        public List<Edge> Edges;
        public float3[] Vertices;
        private readonly float3 _v;

        public Graph(int size)
        {
            Vertices = new float3[size];

            for (var i = 0; i < size; i++)
            {
                _v = new float3();
                Vertices[i] = _v;
            }
        }

        public Graph(List<float3> vertices)
        {
            Vertices = new float3[vertices.Count];

            Vertices = vertices.ToArray();
            _v = Vertices[Vertices.Length - 1];
        }

        public Graph(List<EdgeEndpoint> endpoints)
        {
            Endpoints = endpoints;
            Vertices = new float3[endpoints.Count];

            Vertices = endpoints.Select(x => x.Vertex).ToArray();
            _v = Vertices[Vertices.Length - 1];
        }
    }
}