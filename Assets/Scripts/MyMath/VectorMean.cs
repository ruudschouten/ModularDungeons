using System;
using Unity.Mathematics;
using UnityEngine;

namespace MyMath
{
    [Serializable]
    public struct VectorMean
    {
        public float Total;
        public float Mean;
        public float Min;
        public float Max;

        private int _count;
    
        public float CalculateMeans()
        {
            Mean = Total / _count;

            return Mean;
        }

        public void Add(Vector3 vector)
        {
            var collective = vector.x + vector.y + vector.z;
            if (math.abs(Min) < 0.01f || collective < Min)
            {
                Min = collective;
            }

            if (collective > Max)
            {
                Max = collective;
            }
        
            Total += collective;
            _count++;
        }
    }
}