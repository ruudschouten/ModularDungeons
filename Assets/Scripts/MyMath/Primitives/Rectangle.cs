using System;
using Generation;
using Unity.Mathematics;
using UnityEngine;

namespace MyMath.Primitives
{
    [Serializable]
    public class Rectangle
    {
        /// <summary>
        /// Bottom left point
        /// </summary>
        public float3 A;

        /// <summary>
        /// Top left point
        /// </summary>
        public float3 B;

        /// <summary>
        /// Top right point
        /// </summary>
        public float3 C;

        /// <summary>
        /// Bottom right point
        /// </summary>
        public float3 D;

        /// <summary>
        /// Center of the rectangle
        /// </summary>
        public float3 Center;

        public float3 LeftCenter => (A + B) / 2;
        public float3 RightCenter => (C + D) / 2;
        public float3 TopCenter => (B + C) / 2;
        public float3 BottomCenter => (A + D) / 2;

        public float3 CornerFromIndex(int i)
        {
            switch (i)
            {
                case 0: return A;
                case 1: return B;
                case 2: return C;
                case 3: return D;
            }

            return new float3();
        }

        public float3 CenterFromIndex(int i)
        {
            switch (i)
            {
                case 0: return LeftCenter;
                case 1: return RightCenter;
                case 2: return TopCenter;
                case 3: return BottomCenter;
            }

            return new float3();
        }

        public RectangleSide SideFromIndex(int i)
        {
            switch (i)
            {
                case 0: return RectangleSide.BottomLeft;
                case 1: return RectangleSide.TopLeft;
                case 2: return RectangleSide.TopRight;
                case 3: return RectangleSide.BottomRight;
            }

            return RectangleSide.None;
        }

        /// <summary>
        /// Create a rectangle using 4 points
        /// </summary>
        /// <param name="a">Bottom left point</param>
        /// <param name="b">Top left point</param>
        /// <param name="c">Top right point</param>
        /// <param name="d">Bottom right point</param>
        /// <param name="center">Center of the entire center</param>
        public Rectangle(float3 a, float3 b, float3 c, float3 d, float3 center)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            Center = center;
        }
    }
}