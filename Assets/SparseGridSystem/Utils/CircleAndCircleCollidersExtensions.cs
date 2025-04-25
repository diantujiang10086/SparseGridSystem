using System.Runtime.CompilerServices;
using Unity.Mathematics;

internal static class CircleAndCircleCollidersExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Overlap(in this CircleCollider a, in CircleCollider b)
    {
        float2 delta = a.header.position - b.header.position;
        float distanceSq = math.lengthsq(delta);
        float radiusA = a.header.size.x * 0.5f;
        float radiusB = b.header.size.x * 0.5f;
        float radiusSum = radiusA + radiusB;
        return distanceSq <= radiusSum * radiusSum;
    }

}

