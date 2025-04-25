using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

[StructLayout(LayoutKind.Sequential, Pack = 4)]

internal struct Collider : IEquatable<Collider>
{
    public int instanceId;
    public int layer;
    public int colliderLayer;
    public int colliderType;
    public int colliderColliderType;
    public int isEnableColliderDetection;
    public ColliderShape colliderShape;
    public float2 position;
    public float2 size;

    public bool Equals(Collider other)
    {
        return instanceId == other.instanceId;
    }

    public override bool Equals(object obj)
    {
        return obj is Collider other && Equals(other);
    }

    public override int GetHashCode()
    {
        return instanceId;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEnableColliderDetection()
    {
        return isEnableColliderDetection == 1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckCollideType()
    {
        return colliderColliderType != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanCollideWith(in Collider other)
    {
        return (colliderLayer & other.layer) != 0 &&
               (other.colliderLayer & layer) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanCollideTypeWith(in Collider other)
    {
        return (colliderColliderType & other.colliderType) != 0 &&
               (other.colliderColliderType & colliderType) != 0;
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
