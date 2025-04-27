using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SparseGrid
{
    public static class CollisionHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEqual(float a, float b)
        {
            const float epsilon = 1e-5f;
            return math.abs(a - b) < epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AABBOverlap(in Collider a, in Collider b)
        {
            float2 aHalf = a.header.size * 0.5f;
            float2 bHalf = b.header.size * 0.5f;

            float2 aMin = a.header.position - aHalf;
            float2 aMax = a.header.position + aHalf;
            float2 bMin = b.header.position - bHalf;
            float2 bMax = b.header.position + bHalf;

            return !(aMax.x < bMin.x || aMin.x > bMax.x ||
                     aMax.y < bMin.y || aMin.y > bMax.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap(ref Collider a, ref Collider b)
        {
            ColliderShape shapeA = a.header.colliderShape;
            if (a.header.colliderShape == ColliderShape.Box)
            {
                ref readonly var collider = ref UnsafeUtility.As<Collider, BoxCollider>(ref a);
                return collider.Overlap(ref b);
            }
            if (a.header.colliderShape == ColliderShape.Circle)
            {
                ref readonly var collider = ref UnsafeUtility.As<Collider, CircleCollider>(ref a);
                return collider.Overlap(ref b);
            }

            return false;
        }

        public static bool Overlap(this BoxCollider a, ref Collider b)
        {
            if (b.header.colliderShape == ColliderShape.Box)
            {
                //if (AlmostEqual(a.header.angle, 0) && AlmostEqual(b.header.angle, 0))
                //    return true;

                ref readonly var collider = ref UnsafeUtility.As<Collider, BoxCollider>(ref b);
                return a.Overlap(in collider);
            }
            else if (b.header.colliderShape == ColliderShape.Circle)
            {
                ref readonly var collider = ref UnsafeUtility.As<Collider, CircleCollider>(ref b);
                return a.Overlap(in collider);
            }
            return false;
        }

        public static bool Overlap(this CircleCollider a, ref Collider b)
        {
            if (b.header.colliderShape == ColliderShape.Box)
            {
                ref readonly var collider = ref UnsafeUtility.As<Collider, BoxCollider>(ref b);
                return collider.Overlap(in a);
            }
            else if (b.header.colliderShape == ColliderShape.Circle)
            {
                ref readonly var collider = ref UnsafeUtility.As<Collider, CircleCollider>(ref b);
                return a.Overlap(in collider);
            }
            return false;
        }
    }
}