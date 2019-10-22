using Unity.Mathematics;
using UnityEngine;

namespace MyMath
{
    public static class Round
    {
        public static float Float(float f, float tileSize)
        {
            return (math.round((f + tileSize - 1) / tileSize) * tileSize);
        }

        public static Vector3 Vector3(Vector3 vector, float tileSize)
        {
            return new Vector3(
                Float(vector.x, tileSize),
                Float(vector.y, tileSize),
                Float(vector.z, tileSize));
        }
    }
}