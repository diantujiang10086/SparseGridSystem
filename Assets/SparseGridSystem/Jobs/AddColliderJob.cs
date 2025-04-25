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

        collider.GetGrid(cellSize, out var min, out var max);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                gridMap.Add(new int2(x, y), instanceId);
            }
        }
    }
}
