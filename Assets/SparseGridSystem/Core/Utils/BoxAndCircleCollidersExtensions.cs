using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SparseGrid
{
    internal static class BoxAndCircleCollidersExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap(in this BoxCollider box, in CircleCollider circle)
        {
            float2 circlePos = circle.header.position + circle.center;
            float2 boxPos = box.header.position + box.center;
            float2 localPos = circlePos - boxPos;

            float angle = box.header.angle;
            float cos = math.cos(-angle);
            float sin = math.sin(-angle);

            float2 rotated = new float2(
                localPos.x * cos - localPos.y * sin,
                localPos.x * sin + localPos.y * cos
            );

            float2 halfSize = box.size * 0.5f;
            float2 clamped = math.clamp(rotated, -halfSize, halfSize);

            float2 closest = boxPos + new float2(
                clamped.x * cos + clamped.y * sin,
                -clamped.x * sin + clamped.y * cos
            );

            float2 delta = circlePos - closest;

            return math.lengthsq(delta) <= circle.radius * circle.radius;
        }

    }
}