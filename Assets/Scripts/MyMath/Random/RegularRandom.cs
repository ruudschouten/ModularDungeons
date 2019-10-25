namespace MyMath.Random
{
    public class RegularRandom : BaseRandom
    {
        public RegularRandom(uint seed) : base(seed)
        {
        }
        
        public override int NextInt(int min, int max)
        {
            return Random.NextInt(min, max);
        }

        public override float NextFloat(float min, float max)
        {
            return Random.NextFloat(min, max);
        }
    }
}