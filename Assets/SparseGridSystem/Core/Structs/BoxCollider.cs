using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SparseGrid
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BoxCollider
    {
        public ColliderHeader header;
        public float2 center;
        public float2 size;
    }
}