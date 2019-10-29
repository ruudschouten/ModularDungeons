using System.Collections.Generic;
using System.Linq;
using System.Text;
using Generation.Dungeon.Parts;
using MyMath.Primitives;
using MyMath.SpanningTree;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

namespace Generation.Dungeon
{
    public class PathwayGenerator : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private Pathway prefab;
        [SerializeField] private Transform pathwayContainer;
        [SerializeField] private float pathWidth;
        [SerializeField] [MinValue(3)] private int rows;
        [SerializeField] [MinValue(2)] private int columns;
        [SerializeField] private bool pathShouldHaveMinLength;
        [ShowIf("pathShouldHaveMinLength")] [SerializeField] private float minLengthForPath;
        [SerializeField] private float minLengthForCorner;
        [SerializeField] private bool addBranchingPaths;
        [ShowIf("addBranchingPaths")] [SerializeField] private int branchingPaths;

        [BoxGroup("Colours")] [SerializeField] private Color spanningTreeColour;
        [BoxGroup("Colours")] [SerializeField] private Color pathwayColour;
        [BoxGroup("Colours")] [SerializeField] private Color verticesColour;

        [BoxGroup("Debugging")] [SerializeField] private bool renderSpanningTree;
        [BoxGroup("Debugging")] [SerializeField] private bool renderPathways;
        [BoxGroup("Debugging")] [SerializeField] private bool renderVertices;
        [BoxGroup("Debugging")] [ShowIf("renderVertices")] [SerializeField] private float verticesSize = 0.5f;

        #endregion
        
        public Transform PathContainer => pathwayContainer;

        private readonly MeshCreator _meshCreator = new MeshCreator();
        private readonly Kruskal _kruskal = new Kruskal();
        private List<Vector3> _vertices;

        private List<Edge> _spanningTree;
        private List<Edge> _pathways;

        public void GeneratePathways(List<EdgeEndpoint> endpoints)
        {
            Initialize();

            _spanningTree = _kruskal.Generate(endpoints, addBranchingPaths, branchingPaths);
            _pathways = CreatePathways(_spanningTree);
            DrawMesh(_pathways);
        }

        private void Initialize()
        {
            _vertices = new List<Vector3>();
            _spanningTree = new List<Edge>();
            _pathways = new List<Edge>();
        }

        private void DrawMesh(List<Edge> edges)
        {
            foreach (var edge in edges)
            {
                TurnPathwayIntoMesh(edge);
            }
        }

        private List<Edge> CreatePathways(List<Edge> edges)
        {
            var pathways = new List<Edge>();
            foreach (var edge in edges)
            {
                // Get the closets points from the start and the end edge endpoint
                var s = GetClosestPoint(edge.Start, edge.End.Tile.BottomRectangle.Center);
                var e = GetClosestPoint(edge.End, s);

                if (pathShouldHaveMinLength)
                {
                    var meetsMinLength = MeetsHorizontalRequirements(s, e, minLengthForPath);
                    if (!meetsMinLength) continue;
                }
                
                pathways.AddRange(CreateEdges(edge, s, e));
            }

            return pathways;
        }

        private List<Edge> CreateEdges(Edge edge, float3 closestStart, float3 closestEnd)
        {
            var edges = new List<Edge>();

            var start = edge.Start;
            var end = edge.End;
            
            var startPoint = new EdgeEndpoint(closestStart, start.Tile);
            var endPoint = new EdgeEndpoint(closestEnd, end.Tile);
            
            var corners = CreateCorners(closestStart, closestEnd, out var skipCorner);

            if (skipCorner)
            {
                edges.Add(new Edge(startPoint, endPoint));
                return edges;
            }
            
            var horizontal = MeetsHorizontalRequirements(closestStart, closestEnd, minLengthForCorner);
            if (horizontal)
            {
                var cornerPoint = new EdgeEndpoint(corners[0]);
                edges.Add(new Edge(startPoint, cornerPoint));
                    
                var vertical = MeetsVerticalRequirements(closestStart, closestEnd, corners, minLengthForCorner);
                if (!vertical)
                {
                    edges.Add(new Edge(cornerPoint, endPoint));
                    return edges;
                }
                
                // Only add a secondary point for the corner if the vertical requirements are met
                var secondaryPoint = new EdgeEndpoint(corners[1]);
                
                // Recheck which point is closer from the newest point
                end.UsedCenters.Remove(closestEnd);
                closestEnd = GetClosestPoint(end, corners[1]);
                // Reassign positions
                endPoint = new EdgeEndpoint(closestEnd, end.Tile);
                    
                edges.Add(new Edge(cornerPoint, secondaryPoint));
                edges.Add(new Edge(secondaryPoint, endPoint));
                return edges;
            }
            edges.Add(new Edge(startPoint, endPoint));
            return edges;
        }

        /// <summary>
        /// Returns a collection of positions which are used to create a corner.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="shouldSkipCorner">Should this corner be skipped</param>
        /// <returns></returns>
        private List<float3> CreateCorners(float3 start, float3 end, out bool shouldSkipCorner)
        {
            shouldSkipCorner = false;
            
            var y = (start.y + end.y) / 2f;
            var corners = new List<float3>();
            var corner = new float3(start.x, y, end.z);
            corners.Add(corner);
            if (start.x < end.x) // if s is closer to the origin than e
            {
                if (start.z < end.z) // if s is closer to the right of the origin than e
                {
                    corners.Add(new float3(corner.x + pathWidth, corner.y, end.z));
                }
                else if (start.z > end.z) // if s if closer to the left of the origin than e
                {
                    corners.Add(new float3(end.x + pathWidth, corner.y, start.z));
                }
                else // if s is and e have the same distance from origin
                {
                    shouldSkipCorner = true;
                }
            }
            else if (end.x < start.x) // if e is closer to the origin than s
            {
                if (end.z < start.z) // if e is closer to the right of the origin than s
                {
                    corners.Add(new float3(corner.x + pathWidth, corner.y, end.z));
                }
                else if (end.z > start.z) // if e is closer to the left of the origin than s
                {
                    corners.Add(new float3(end.x + pathWidth, corner.y, start.z));
                }
                else
                {
                    shouldSkipCorner = true;
                }
            }
            else
            {
                shouldSkipCorner = true;
            }

            return corners;
        }

        /// <summary>
        /// Loops through positions in the <paramref name="endpoint"/> to find which position is closest to <paramref name="point"/>
        /// </summary>
        /// <param name="endpoint">Endpoint which contains a closest position</param>
        /// <param name="point">Point to check distance to</param>
        /// <param name="skipUsedSpots">Should used spots be skipped</param>
        /// <returns>The closest position</returns>
        private float3 GetClosestPoint(EdgeEndpoint endpoint, float3 point, bool skipUsedSpots = true)
        {
            var closestDist = float.MaxValue;
            var closest = new float3(float.MaxValue, float.MaxValue, float.MaxValue);

            // Loop through the four sides
            for (var i = 0; i < 4; i++)
            {
                var p = endpoint.Tile.BottomRectangle.CenterFromIndex(i);
                var dist = math.distance(p, point);
                if (dist > closestDist) continue;
                if (skipUsedSpots && endpoint.UsedCenters.Contains(p)) continue;

                closestDist = dist;
                closest = p;
            }

            if (skipUsedSpots)
            {
                endpoint.UsedCenters.Add(closest);
            }

            return closest;
        }
        
        /// <summary>
        /// Checks if the vertical length between <paramref name="start"/> and <paramref name="end"/> meet the <paramref name="requirement"/>
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="corners">Corner positions, right now only checks for two corners</param>
        /// <param name="requirement">Required length that should be met</param>
        /// <returns></returns>
        private bool MeetsVerticalRequirements(float3 start, float3 end, List<float3> corners, float requirement)
        {
            var yLength = math.distance(start.y, end.y);
            if (corners.Any())
            {
                var yStartLength = math.distance(start.y, corners[0].y);
                var yEndLength = math.distance(corners[corners.Count - 1].y, end.y);

                if (yLength >= 1f)
                {
                    if (yStartLength >= requirement || yEndLength >= requirement)
                    {
                        return true;
                    }
                }
            }
            else
            {
                return yLength > requirement;
            }

            return false;
        }
        
        /// <summary>
        /// Checks if the horizontal length between <paramref name="start"/> and <paramref name="end"/> meet the <paramref name="requirement"/>
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="requirement">Required length that should be met</param>
        /// <returns></returns>
        private bool MeetsHorizontalRequirements(float3 start, float3 end, float requirement)
        {
            var xLength = math.distance(start.x, end.x);
            var zLength = math.distance(start.z, end.z);

            if (xLength >= requirement || zLength >= requirement)
            {
                return true;
            }

            return false;
        }

        private void TurnPathwayIntoMesh(Edge edge)
        {
            var vertices = _meshCreator.CalculateVertices(rows, columns, pathWidth, edge, out var uvs, out var tangents);
            var triangles = _meshCreator.CalculateTriangles(rows, columns);

            // Don't create a new path if any of the two methods returned a empty array
            if (vertices.Length == 0 || triangles.Length == 0) return;

            var pathName = new StringBuilder("Path | ");
            pathName.Append(edge.Start.Tile != null ? $"{edge.Start.Tile.Id}" : "0");
            pathName.Append(edge.End.Tile != null ? $" - {edge.End.Tile.Id}" : " - 0");

            var path = Instantiate(prefab, pathwayContainer);
            path.name = pathName.ToString();
            path.Weight = edge.Weight;
            path.Mesh = _meshCreator.CreateGridMesh(vertices, triangles, uvs, tangents);

            _vertices.AddRange(vertices);
        }

        private void DrawEdgeGizmos(IReadOnlyCollection<Edge> edges, Color colour)
        {
            if (edges == null || !edges.Any()) return;

            Gizmos.color = colour;
            foreach (var edge in edges)
            {
                Gizmos.DrawLine(edge.V1, edge.V2);
            }
        }

        private void DrawPointGizmos(IReadOnlyCollection<Vector3> vertices, Color colour)
        {
            if (vertices == null) return;

            Gizmos.color = colour;
            foreach (var t in vertices)
            {
                Gizmos.DrawSphere(t, verticesSize);
            }
        }

        private void OnDrawGizmos()
        {
            if (renderVertices)
            {
                DrawPointGizmos(_vertices, verticesColour);
            }

            if (renderSpanningTree)
            {
                DrawEdgeGizmos(_spanningTree, spanningTreeColour);
            }

            if (renderPathways)
            {
                DrawEdgeGizmos(_pathways, pathwayColour);
            }
        }
    }
}