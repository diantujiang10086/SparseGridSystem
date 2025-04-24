using System;

internal struct InstanceIdWithIndex : IEquatable<InstanceIdWithIndex>
{
    public int instanceId;
    public int colliderIndex;

    public bool Equals(InstanceIdWithIndex other)
    {
        return instanceId == other.instanceId;
    }
    public override int GetHashCode()
    {
        return instanceId;
    }
}
