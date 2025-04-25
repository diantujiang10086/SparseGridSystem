using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile()]
internal struct RemoveIndexsJob : IJobParallelFor
{
    public float cellSize;
    [ReadOnly] public NativeList<Collider> colliders;
    [ReadOnly] public NativeArray<int>.ReadOnly removeArray;
    [ReadOnly] public NativeParallelHashMap<int, int> idToIndex;
    [ReadOnly] public NativeParallelHashMap<int, int> colliderDetectionMap;
    public NativeParallelHashSet<InstanceIdWithIndex>.ParallelWriter removeColliderIndexs;
    public NativeParallelHashSet<int3>.ParallelWriter removeGridInstanceIds;
    public NativeParallelHashSet<int>.ParallelWriter removeColliderDetectionIndexs;
    public void Execute(int index)
    {
        int instanceId = removeArray[index];
        if (!idToIndex.TryGetValue(instanceId, out var colliderIndex))
            return;
        removeColliderIndexs.Add(new InstanceIdWithIndex { colliderIndex = colliderIndex, instanceId = instanceId });
        var collider = colliders[colliderIndex];

        if(collider.IsEnableColliderDetection())
        {
            if(colliderDetectionMap.TryGetValue(collider.instanceId, out var colliderDetectionIndex))
            {
                removeColliderDetectionIndexs.Add(colliderDetectionIndex);
            }
        }

        collider.GetGrid(cellSize, out var min, out var max);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                removeGridInstanceIds.Add(new int3(x, y, collider.instanceId));
            }
        }
    }
}
