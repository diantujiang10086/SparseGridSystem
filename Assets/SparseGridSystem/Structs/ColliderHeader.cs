using System.Runtime.InteropServices;
using Unity.Mathematics;
public enum ColliderShape : int
{
    None = 0,
    Box = 1,
    Circle = 2,
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct ColliderHeader
{
    public int instanceId;
    public int layer;
    public int colliderLayer;
    public int colliderType;
    public int colliderColliderType;
    public int isEnableColliderDetection;
    public ColliderShape colliderShape;
    public float angle;
    public float2 position;
    public float2 size;
}
