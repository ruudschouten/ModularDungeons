using System;
using Unity.Mathematics;

namespace MyMath.Primitives
{
    public class Simplex
    {
        /// <summary>
        /// The simplexs adjacent to this simplex
        /// Will be a triangle and it with have three adjacent triangles joining it.
        /// </summary>
        public Simplex[] Adjacent { get; }

        /// <summary>
        /// The vertices that make up the simplex.
        /// Will be 3 vertices making a triangle.
        /// </summary>
        public float3[] Vertices { get; }

        /// <summary>
        /// The simplexs normal.
        /// </summary>
        public float[] Normal { get; }

        /// <summary>
        /// The simplexs centroid.
        /// </summary>
        public float[] Centroid { get; }

        /// <summary>
        /// The simplexs offset from the origin.
        /// </summary>
        public float Offset { get; set; }

        public int Tag { get; set; }

        public int Dimension = 3;

        public bool IsNormalFlipped { get; set; }

        public Simplex()
        {
            Adjacent = new Simplex[Dimension];
            Normal = new float[Dimension];
            Centroid = new float[Dimension];
            Vertices = new float3[Dimension];
        }

        public float Dot(float3 v)
        {
            var dp = 0.0f;

            dp += Normal[0] * v.x;
            dp += Normal[1] * v.y;
            dp += Normal[2] * v.z;
            
            return dp;
        }

        public bool Remove(Simplex simplex)
        {
            if (simplex == null) return false;
            var n = Adjacent.Length;

            for (var i = 0; i < n; i++)
            {
                if (Adjacent[i] == null) continue;

                if (!ReferenceEquals(Adjacent[i], simplex)) continue;
                Adjacent[i] = null;
                return true;
            }

            return false;
        }

        public void CalculateNormal()
        {
            var ntX = new float[3];
            var ntY = new float[3];
            Subtract(Vertices[1], Vertices[0], ntX);
            Subtract(Vertices[2], Vertices[1], ntY);

            var nx = ntX[1] * ntY[2] - ntX[2] * ntY[1];
            var ny = ntX[2] * ntY[0] - ntX[0] * ntY[2];
            var nz = ntX[0] * ntY[1] - ntX[1] * ntY[0];

            var norm = (float) Math.Sqrt(nx * nx + ny * ny + nz * nz);

            var f = 1.0f / norm;
            Normal[0] = f * nx;
            Normal[1] = f * ny;
            Normal[2] = f * nz;
        }
        
        private void Subtract(float3 x, float3 y, float[] target)
        {
            for (var i = 0; i < Dimension; i++)
            {
                target[i] = x[i] - y[i];
            }
        }

        public void CalculateCentroid()
        {
            CalculateCentroid3D();
        }

        private void CalculateCentroid3D()
        {
            Centroid[0] = (Vertices[0].x + Vertices[1].x + Vertices[2].x) / 3.0f;
            Centroid[1] = (Vertices[0].y + Vertices[1].y + Vertices[2].y) / 3.0f;
            Centroid[2] = (Vertices[0].z + Vertices[1].z + Vertices[2].z) / 3.0f;
        }
        public override string ToString()
        {
            var indexes = "";

            var d = Dimension;
            for (var i = 0; i < d; i++)
            {
                indexes += Vertices[i];
                if (i != d - 1) indexes += ",";
            }

            return $"[Simplex: Dimension={Dimension},  Vertices={indexes}]";
        }
    }
}