using System.Runtime.InteropServices;
using Unity.Mathematics;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct BoxCollider
{
    public ColliderHeader header;
    public float2 center;
    public float2 size;
}