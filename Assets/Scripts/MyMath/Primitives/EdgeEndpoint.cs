using System.Collections.Generic;
using Generation.Dungeon.Parts;
using Unity.Mathematics;

namespace MyMath.Primitives
{
    public class EdgeEndpoint
    {
        public float3 Vertex { get; }
        public Tile Tile { get; }

        public List<float3> UsedCenters { get; } = new List<float3>();

        public EdgeEndpoint(float3 vertex)
        {
            Vertex = vertex;
        }

        public EdgeEndpoint(float3 vertex, Tile tile) : this(vertex)
        {
            Tile = tile;
        }
    }
}