using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SparseGrid
{
    internal static class CircleAndCircleCollidersExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap(in this CircleCollider a, in CircleCollider b)
        {
            float2 delta = (a.header.position + a.center) - (b.header.position + b.center);
            float distanceSq = math.lengthsq(delta);
            float radiusA = a.radius;
            float radiusB = b.radius;
            float radiusSum = radiusA + radiusB;
            return distanceSq <= radiusSum * radiusSum;
        }

    }
}