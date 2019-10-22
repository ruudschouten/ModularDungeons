using Generation;
using Unity.Mathematics;

namespace MyMath.Primitives
{
    public class Edge
    {
        public EdgeEndpoint Start { get; }
        public EdgeEndpoint End { get; }
        public float3 V1 => Start.Vertex;
        public float3 V2 => End.Vertex;
        public float Weight { get; }

        public Edge(float3 start, float3 end) : this(new EdgeEndpoint(start), new EdgeEndpoint(end))
        {
        }

        public Edge(EdgeEndpoint start, EdgeEndpoint end)
        {
            Start = start;
            End = end;

            Weight = math.distance(V1, V2);
        }

        public Edge(EdgeEndpoint start, EdgeEndpoint end, float weight)
        {
            Start = start;
            End = end;

            Weight = weight;
        }
    }
}