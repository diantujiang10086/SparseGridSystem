using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SparseGrid
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CircleCollider
    {
        public ColliderHeader header;
        public RectCollider rectCollider;
        public float2 center;
        public float radius;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private float placeholder;
    }
}