using Unity.Mathematics;

public static class Helper
{
    public static int2 WorldToGridPos(float2 worldPos, float cellSize)
    {
        return (int2)math.floor(worldPos / cellSize);
    }
}
