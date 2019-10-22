using Unity.Mathematics;

namespace MyMath.Primitives
{
    public class Subset
    {
        public float3 Parent { get; set; }
        /// <summary>
        /// Rank defines at what level in the tree this Subset is.
        /// 0 = Outer leaf, 1 = Branch connecting to leaf, and so on
        /// </summary>
        public int Rank { get; set; }
    }
}