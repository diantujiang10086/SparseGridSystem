using System.Runtime.CompilerServices;
using Unity.Mathematics;

internal static class CollisionHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Overlap(in Collider a, in Collider b)
    {
        ColliderShape shapeA = a.colliderShape;
        ColliderShape shapeB = b.colliderShape;

        if (shapeA == ColliderShape.Box && shapeB == ColliderShape.Box)
            return AABBOverlap(in a, in b);

        if (shapeA == ColliderShape.Circle && shapeB == ColliderShape.Circle)
            return CircleOverlap(in a, in b);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AABBOverlap(in Collider a, in Collider b)
    {
        float2 aHalf = a.size * 0.5f;
        float2 bHalf = b.size * 0.5f;

        float2 aMin = a.position - aHalf;
        float2 aMax = a.position + aHalf;
        float2 bMin = b.position - bHalf;
        float2 bMax = b.position + bHalf;

        return !(aMax.x < bMin.x || aMin.x > bMax.x ||
                 aMax.y < bMin.y || aMin.y > bMax.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleOverlap(in Collider a, in Collider b)
    {
        float2 delta = a.position - b.position;
        float distanceSq = math.lengthsq(delta);
        float radiusA = a.size.x * 0.5f;
        float radiusB = b.size.x * 0.5f;
        float radiusSum = radiusA + radiusB;
        return distanceSq <= radiusSum * radiusSum;
    }

}
