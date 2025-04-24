using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

[BurstCompile()]
internal struct UpdateColliderJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<UpdateCollider>.ReadOnly updateColliders;
    [ReadOnly] public NativeParallelHashMap<int, int> idToIndex;
    [NativeDisableParallelForRestriction] public NativeList<Collider> colliders;
    public void Execute(int index)
    {
        var updateCollider = updateColliders[index];
        if (!idToIndex.TryGetValue(updateCollider.instanceId, out var colliderIndex))
            return;

        Collider collider = colliders[colliderIndex];
        collider.position = updateCollider.position;
        colliders[colliderIndex] = collider;
    }
}
