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
    public NativeParallelHashSet<InstanceIdWithIndex>.ParallelWriter removeColliderIndexs;
    public NativeParallelHashSet<int3>.ParallelWriter removeGridInstanceIds;
    public void Execute(int index)
    {
        int instanceId = removeArray[index];
        if (!idToIndex.TryGetValue(instanceId, out var colliderIndex))
            return;
        removeColliderIndexs.Add(new InstanceIdWithIndex { colliderIndex = colliderIndex, instanceId = instanceId });
        var collider = colliders[colliderIndex];

        var half = collider.size * 0.5f;
        int2 minGrid = Helper.WorldToGridPos(collider.position - half, cellSize);
        int2 maxGrid = Helper.WorldToGridPos(collider.position + half, cellSize);

        for (int x = minGrid.x; x <= maxGrid.x; x++)
        {
            for (int y = minGrid.y; y <= maxGrid.y; y++)
            {
                removeGridInstanceIds.Add(new int3(x, y, collider.instanceId));
            }
        }
    }
}
