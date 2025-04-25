using System.Runtime.CompilerServices;
using Unity.Mathematics;

/// <summary>
/// GPT生成未测试
/// </summary>
internal static class BoxAndCircleCollidersExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Overlap(in this BoxCollider box, in CircleCollider circle)
    {
        float2 dir = new float2(math.cos(-box.header.angle), math.sin(-box.header.angle));
        float2 perp = new float2(-dir.y, dir.x);

        float2 localPos = circle.header.position - box.header.position;
        float2 rotated = new float2(math.dot(localPos, dir), math.dot(localPos, perp));

        float2 half = box.header.size * 0.5f;
        float2 clamped = math.clamp(rotated, -half, half);

        float2 closest = box.header.position + dir * clamped.x + perp * clamped.y;
        float2 delta = circle.header.position - closest;
        float radius = circle.header.size.x * 0.5f;

        return math.lengthsq(delta) <= radius * radius;
    }
}

