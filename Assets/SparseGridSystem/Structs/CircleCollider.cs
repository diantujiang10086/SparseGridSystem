using System.Runtime.InteropServices;
using Unity.Mathematics;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct CircleCollider
{
    public ColliderHeader header;
    public float2 center;
    public float radius;
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private float placeholder;
}
