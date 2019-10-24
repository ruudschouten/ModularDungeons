namespace MyMath.Random
{
    public class RegularRandom : IRandom
    {
        private Unity.Mathematics.Random _random;

        public RegularRandom(uint seed)
        {
            _random = new Unity.Mathematics.Random(seed);
        }

        public int NextInt()
        {
            return NextInt(int.MaxValue);
        }

        public int NextInt(int max)
        {
            return NextInt(0, max);
        }

        public int NextInt(int min, int max)
        {
            return _random.NextInt(min, max);
        }

        public float NextFloat()
        {
            return NextFloat(float.MaxValue);
        }

        public float NextFloat(float max)
        {
            return NextFloat(0, max);
        }

        public float NextFloat(float min, float max)
        {
            return _random.NextFloat(min, max);
        }
    }
}