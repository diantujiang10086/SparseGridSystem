using System.Runtime.CompilerServices;
using Unity.Mathematics;

internal static class Helper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int2 WorldToGridPos(float2 worldPos, float cellSize)
    {
        return (int2)math.floor(worldPos / cellSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GetGrid(this Collider collider, float cellSize, out int2 min, out int2 max)
    {
        var halfSize = collider.size * 0.5f;
        min = Helper.WorldToGridPos(collider.position - halfSize, cellSize);
        max = Helper.WorldToGridPos(collider.position + halfSize, cellSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GetGrid(float2 position, float2 size, float cellSize, out int2 min, out int2 max)
    {
        var halfSize = size * 0.5f;
        min = Helper.WorldToGridPos(position - halfSize, cellSize);
        max = Helper.WorldToGridPos(position + halfSize, cellSize);
    }
}
