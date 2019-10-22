using System;
using System.Collections.Generic;
using System.Linq;
using MyMath.Primitives;
using Unity.Mathematics;
using UnityEngine;

namespace MyMath.SpanningTree
{
    public class Kruskal
    {
        public List<Edge> Generate(List<EdgeEndpoint> endpoints, bool addBranching, int branchCount)
        {
            var count = endpoints.Count;

            var edges = CalculateEdges(endpoints);

            var graph = new Graph(endpoints)
            {
                Edges = edges.OrderBy(x => x.Weight).ToList()
            };
            // Sort edges by weight

            // Create subsets to keep track of which vertices have been used and which ones haven't been used in the tree.
            var subsets = CreateSubsets(endpoints, count);

            var result = CalculateResult(count, graph, subsets);

            if (!addBranching) return result;

            var extraEdges = graph.Edges.Skip(result.Count).Take(branchCount);
            result.AddRange(extraEdges);
            return result;
        }

        private List<Edge> CalculateEdges(List<EdgeEndpoint> endpoints)
        {
            // Add edges based on vertices
            var count = endpoints.Count;
            var edges = new List<Edge>();

            for (var i = 0; i < count; i++)
            {
                var vert1 = endpoints[i];
                for (var j = i; j < count; j++)
                {
                    var vert2 = endpoints[j];
                    if (i == j) continue;

                    var ydist = math.distance(vert1.Vertex.y, vert2.Vertex.y);
                    var dist = math.distance(vert1.Vertex, vert2.Vertex);
                    var remainingDist = dist - ydist;
                    
                    var weight = dist;

                    // if y axis is larger than the other distances combined, make it bigger so we won't get weird ladders
                    if (ydist > remainingDist / 2)
                    {
                        weight *= (math.abs(ydist) + 5) * 3;
                    }
                    
                    var edge = new Edge(vert1, vert2, weight);
                    edges.Add(edge);
                }
            }

            return edges;
        }

        private static Subset[] CreateSubsets(List<EdgeEndpoint> endpoints, int count)
        {
            var sub = new Subset[count];
            for (var i = 0; i < count; i++)
            {
                var subSet = new Subset
                {
                    Parent = endpoints[i].Vertex,
                    Rank = 0
                };
                sub[i] = subSet;
            }

            return sub;
        }

        private List<Edge> CalculateResult(int count, Graph graph, Subset[] subsets)
        {
            var result = new List<Edge>();
            var index = 0;
            var resultEdges = 0;
            // Loop through the edges 
            while (resultEdges < count - 1)
            {
                var edge = graph.Edges.ElementAt(index);
                var xParent = FindParent(subsets, edge.V1, graph.Vertices);
                var yParent = FindParent(subsets, edge.V2, graph.Vertices);

                // Check if the parent edges aren't the same, if so this edge hasn't been used in the tree yet.
                if (!xParent.Equals(yParent))
                {
                    result.Add(edge);
                    Union(subsets, xParent, yParent, graph.Vertices);
                    resultEdges++;
                }

                index++;
            }

            return result;
        }

        /// <summary>
        /// Looks for the parent of given vertex. 
        /// </summary>
        /// <param name="subsets">Collection of subsets to check for parents</param>
        /// <param name="vertex">Vertex that needs to get its parent found</param>
        /// <param name="vertices">Collection of vertices which contains 'vertex'</param>
        /// <returns>Either itself, or its parent</returns>
        private float3 FindParent(Subset[] subsets, float3 vertex, float3[] vertices)
        {
            var index = Array.IndexOf(vertices, vertex);
            return FindParent(subsets, vertex, vertices, index);
        }

        /// <summary>
        /// Looks for the parent of given vertex. 
        /// </summary>
        /// <param name="subsets">Collection of subsets to check for parents</param>
        /// <param name="vertex">Vertex that needs to get its parent found</param>
        /// <param name="vertices">Collection of vertices which contains 'vertex'</param>
        /// <param name="index">index of 'vertex' in 'vertices'</param>
        /// <returns>Either itself, or its parent</returns>
        private float3 FindParent(Subset[] subsets, float3 vertex, float3[] vertices, int index)
        {
            // If the Subset at index is equal to vertex, this subset is its own parent, meaning that it hasn't been used yet.
            if (!subsets[index].Parent.Equals(vertex))
            {
                var vertIndex = Array.IndexOf(vertices, subsets.ElementAt(index).Parent);
                subsets[index].Parent = FindParent(subsets, subsets.ElementAt(index).Parent, vertices, vertIndex);
            }

            return subsets[index].Parent;
        }

        /// <summary>
        /// Combines x and y vertices and updates the subsets collection
        /// </summary>
        /// <param name="subsets">Collection of subsets</param>
        /// <param name="x">Vertex that needs to be combined</param>
        /// <param name="y">Other vertex that needs to be combined</param>
        /// <param name="vertices">Collection of vertices that is needed to find 'x' and 'y' index</param>
        private void Union(Subset[] subsets, float3 x, float3 y, float3[] vertices)
        {
            var xParent = FindParent(subsets, x, vertices);
            var yParent = FindParent(subsets, y, vertices);

            var xIndex = Array.IndexOf(vertices, xParent);
            var yIndex = Array.IndexOf(vertices, yParent);

            if (subsets[xIndex].Rank < subsets[yIndex].Rank)
            {
                subsets[xIndex].Parent = yParent;
            }
            else if (subsets[xIndex].Rank > subsets[yIndex].Rank)
            {
                subsets[yIndex].Parent = xParent;
            }
            else
            {
                subsets[yIndex].Parent = xParent;
                subsets[xIndex].Rank++;
            }
        }
    }
}