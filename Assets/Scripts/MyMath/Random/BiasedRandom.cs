namespace MyMath.Random
{
    public class BiasedRandom : IRandom
    {
        private Unity.Mathematics.Random _random;
        private readonly Bias _bias = Bias.Lower;
        private readonly int _rollCount = 4;

        public BiasedRandom()
        {
            _random = new Unity.Mathematics.Random();
        }
        
        public BiasedRandom(uint seed)
        {
            _random = new Unity.Mathematics.Random(seed);
        }

        public BiasedRandom(uint seed, int rollCount) : this(seed)
        {
            _rollCount = rollCount;
        }

        public BiasedRandom( uint seed, int rollCount, Bias bias) : this(seed, rollCount)
        {
            _bias = bias;
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
            var lowest = int.MaxValue;
            var highest = -1;

            for (var i = 0; i < _rollCount; i++)
            {
                var r = _random.NextInt(min, max);
                if (r < lowest)
                {
                    lowest = r;
                }

                if (r > highest)
                {
                    highest = r;
                }
            }

            return _bias == Bias.Lower ? lowest : highest;
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
            var lowest = float.MaxValue;
            var highest = -1f;

            for (var i = 0; i < _rollCount; i++)
            {
                var r = _random.NextFloat(min, max);
                if (r < lowest)
                {
                    lowest = r;
                }

                if (r > highest)
                {
                    highest = r;
                }
            }

            return _bias == Bias.Lower ? lowest : highest;
        }
    }
}