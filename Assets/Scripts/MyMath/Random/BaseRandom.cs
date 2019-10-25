using MyMath.Random.Interface;

namespace MyMath.Random
{
    public abstract class BaseRandom : IRandom
    {
        protected Unity.Mathematics.Random Random;

        protected BaseRandom()
        {
            Random = new Unity.Mathematics.Random();
        }
        
        protected BaseRandom(uint seed)
        {
            Random = new Unity.Mathematics.Random(seed);
        }
        
        public int NextInt()
        {
            return NextInt(0, int.MaxValue);
        }

        public int NextInt(int max)
        {
            return NextInt(0, max);
        }

        public abstract int NextInt(int min, int max);

        public float NextFloat()
        {
            return NextFloat(0, float.MaxValue);
        }

        public float NextFloat(float max)
        {
            return NextFloat(0, max);
        }

        public abstract float NextFloat(float min, float max);
    }
}