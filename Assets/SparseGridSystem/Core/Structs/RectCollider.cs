using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SparseGrid
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RectCollider : IEquatable<RectCollider>
    {
        public int2 min;
        public int2 max;
        public RectCollider(int2 min, int2 max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Equals(RectCollider other)
        {
            return math.all( min == other.min) && math.all(max == other.max);
        }

        public override bool Equals(object obj)
        {
            return obj is RectCollider other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + min.GetHashCode();
                hash = hash * 31 + max.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(RectCollider left, RectCollider right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RectCollider left, RectCollider right)
        {
            return !left.Equals(right);
        }
    }
}