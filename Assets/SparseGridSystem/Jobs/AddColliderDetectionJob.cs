using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct AddColliderDetectionJob : IJobParallelForDefer
{
    public int colliderArrayLength;
    [ReadOnly] public NativeList<int> addColliderDetections;
    [NativeDisableParallelForRestriction] public NativeList<int> colliderDetections;
    public NativeParallelHashMap<int, int>.ParallelWriter colliderDetectionMap;
    public void Execute(int index)
    {
        var instanceId = addColliderDetections[index];
        int colliderDetectionIndex = colliderArrayLength + index;
        colliderDetections[colliderDetectionIndex] = instanceId;
        colliderDetectionMap.TryAdd(instanceId, colliderDetectionIndex);
    }
}
