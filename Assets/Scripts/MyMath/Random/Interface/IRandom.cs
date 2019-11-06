namespace MyMath.Random.Interface
{
    public interface IRandom
    {
        int NextInt();
        int NextInt(int max);
        int NextInt(int min, int max);
        float NextFloat();
        float NextFloat(float max);
        float NextFloat(float min, float max);
    }
}