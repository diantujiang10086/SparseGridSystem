using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile()]
internal struct AddColliderJob : IJobParallelFor
{
    public float cellSize;
    public int arrayLength;
    [ReadOnly] public NativeArray<Collider>.ReadOnly addArray;
    [NativeDisableParallelForRestriction] public NativeList<Collider> colliders;
    public NativeParallelHashMap<int, int>.ParallelWriter idToIndex;
    public NativeParallelMultiHashMap<int2, int>.ParallelWriter gridMap;
    public NativeList<int>.ParallelWriter addColliderDetections;
    public void Execute(int index)
    {
        var collider = addArray[index];
        int instanceId = collider.instanceId;
        var colliderIndex = arrayLength + index;
        colliders[colliderIndex] = collider;
        idToIndex.TryAdd(instanceId, colliderIndex);

        if(collider.IsEnableColliderDetection())
        {
            addColliderDetections.AddNoResize(collider.instanceId);
        }
        
        var half = collider.size * 0.5f;
        int2 minGrid = Helper.WorldToGridPos(collider.position - half, cellSize);
        int2 maxGrid = Helper.WorldToGridPos(collider.position + half, cellSize);

        for (int x = minGrid.x; x <= maxGrid.x; x++)
        {
            for (int y = minGrid.y; y <= maxGrid.y; y++)
            {
                gridMap.Add(new int2(x, y), instanceId);
            }
        }
    }
}
