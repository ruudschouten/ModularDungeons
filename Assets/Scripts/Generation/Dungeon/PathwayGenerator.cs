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
        [SerializeField] private float minLengthForPath;
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
                var start = edge.Start;
                var end = edge.End;

                // Get the closets points from the start and the end edge endpoint
                var s = GetClosestPoint(start, end.Tile.BottomRectangle.Center, true);
                var e = GetClosestPoint(end, s, true);
                
                var meetsMinLength = MeetsHorizontalRequirements(s, e, minLengthForPath);
                if (!meetsMinLength) continue;
                
                var y = (s.y + e.y) / 2f;
                var startPoint = new EdgeEndpoint(s, start.Tile);
                var endPoint = new EdgeEndpoint(e, end.Tile);
                
                var corner = CreateCorners(s, e, y, out var secondaryCorner, out var skipCorner);

                if (skipCorner)
                {
                    pathways.Add(new Edge(startPoint, endPoint));
                    continue;
                }
                // Check if path length is long enough for it to gain a corner
                var horizontal = MeetsHorizontalRequirements(s, e, minLengthForCorner);
                if (horizontal)
                {
                    var cornerPoint = new EdgeEndpoint(corner);
                    pathways.Add(new Edge(startPoint, cornerPoint));
                    
                    var vertical = MeetsVerticalRequirements(s, e, corner, secondaryCorner, minLengthForCorner);
                    // Only add a secondary point for the corner if the vertical requirements are met
                    if (vertical)
                    {
                        var secondaryPoint = new EdgeEndpoint(secondaryCorner);
                    
                        // Recheck which point is closer from the newest point
                        end.UsedCenters.Remove(e);
                        e = GetClosestPoint(end, secondaryCorner, true);
                        // Reassign positions
                        endPoint = new EdgeEndpoint(e, end.Tile);
                        
                        pathways.Add(new Edge(cornerPoint, secondaryPoint));
                        pathways.Add(new Edge(secondaryPoint, endPoint));
                    }
                    else
                    {
                        pathways.Add(new Edge(cornerPoint, endPoint));
                    }
                }
                else
                {
                    pathways.Add(new Edge(startPoint, endPoint));
                }
            }

            return pathways;
        }

        // TODO: Rewrite this with the use of a list
        private float3 CreateCorners(float3 s, float3 e, float y, out float3 secondary, out bool skipCorner)
        {
            var corner = float3.zero;
            secondary = float3.zero;
            skipCorner = false;
            if (s.x < e.x) // if s is closer to the origin than e
            {
                if (s.z < e.z) // if s is closer to the right of the origin than e
                {
                    corner = new float3(s.x, y, e.z);
                    secondary = new float3(corner.x + pathWidth, corner.y, e.z);
                }
                else if (s.z > e.z) // if s if closer to the left of the origin than e
                {
                    corner = new float3(s.x, y, e.z);
                    secondary = new float3(e.x + pathWidth, corner.y, s.z);
                }
                else // if s is and e have the same distance from origin
                {
                    skipCorner = true;
                }
            }
            else if (e.x < s.x) // if e is closer to the origin than s
            {
                if (e.z < s.z) // if e is closer to the right of the origin than s
                {
                    corner = new float3(s.x, y, e.z);
                    secondary = new float3(corner.x + pathWidth, corner.y, e.z);
                }
                else if (e.z > s.z) // if e is closer to the left of the origin than s
                {
                    corner = new float3(s.x, y, e.z);
                    secondary = new float3(e.x + pathWidth, corner.y, s.z);
                }
                else
                {
                    skipCorner = true;
                }
            }
            else
            {
                skipCorner = true;
            }

            return corner;
        }

        private float3 GetClosestPoint(EdgeEndpoint endpoint, float3 point, bool skipUsedSpots)
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

        private bool MeetsVerticalRequirements(float3 start, float3 end, float3 corner, float3 secondaryCorner, float requirement)
        {
            var yStartLength = math.distance(start.y, corner.y);
            var yEndLength = math.distance(secondaryCorner.y, end.y);
            var yLength = math.distance(start.y, end.y);
            if (yLength >= 1f)
            {
                if (yStartLength >= requirement || yEndLength >= requirement)
                {
                    return true;
                }
            }

            return false;
        }
        
        private bool MeetsHorizontalRequirements(float3 start, float3 end, float requirement)
        {
            var xLength = math.distance(start.x, end.x);
            var zLength = math.distance(start.x, end.x);

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