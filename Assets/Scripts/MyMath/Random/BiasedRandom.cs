using MyMath.Random.Types;

namespace MyMath.Random
{
    public class BiasedRandom : BaseRandom
    {
        private readonly Bias _bias = Bias.Lower;
        private readonly int _rollCount;

        public BiasedRandom(uint seed, int rollCount) : base(seed)
        {
            _rollCount = rollCount;
        }

        public BiasedRandom(uint seed, int rollCount, Bias bias) : this(seed, rollCount)
        {
            _bias = bias;
        }

        public override int NextInt(int min, int max)
        {
            return (int) Roll(min, max);
        }

        public override float NextFloat(float min, float max)
        {
            return Roll(min, max);
        }

        private float Roll(float min, float max)
        {
            var lowest = float.MaxValue;
            var highest = -1f;
            for (var i = 0; i < _rollCount; i++)
            {
                var r = Random.NextFloat(min, max);
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