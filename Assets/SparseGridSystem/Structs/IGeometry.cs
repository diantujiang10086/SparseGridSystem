using Unity.Mathematics;

public interface IGeometry
{
}
public class BoxGeometry : IGeometry
{
    public float2 center;
    public float2 size;
}

public class CircleGeometry : IGeometry
{
    public float2 center;
    public float radius;
}