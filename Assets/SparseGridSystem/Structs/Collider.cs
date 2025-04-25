using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

[StructLayout(LayoutKind.Sequential, Pack = 4)]

internal struct Collider : IEquatable<Collider>
{
    public ColliderHeader header;
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private float4 placeholder;

    public bool Equals(Collider other)
    {
        return header.instanceId == other.header.instanceId;
    }

    public override bool Equals(object obj)
    {
        return obj is Collider other && Equals(other);
    }

    public override int GetHashCode()
    {
        return header.instanceId;
    }

    public static bool operator ==(Collider left, Collider right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Collider left, Collider right)
    {
        return !left.Equals(right);
    }
}

