using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile()]
internal struct UpdateGridJob : IJobParallelFor
{
    [ReadOnly] public float cellSize;
    [ReadOnly] public NativeList<Collider> colliders;
    [ReadOnly] public NativeArray<UpdateCollider>.ReadOnly updateColliders;
    [ReadOnly] public NativeParallelHashMap<int, int> idToIndex;
    public NativeParallelMultiHashMap<int2, int>.ParallelWriter gridMap;
    public NativeParallelHashSet<int3>.ParallelWriter removeGrid;
    public void Execute(int index)
    {
        var updateCollider = updateColliders[index];
        Collider collider;
        if(!idToIndex.TryGetValue(updateCollider.instanceId, out var colliderIndex))
            return;

        collider = colliders[colliderIndex];

        var curPosition = collider.header.position;
        var newPosition = updateCollider.position;
        if(math.all(curPosition == newPosition))
            return;

        var halfSize = collider.header.size * 0.5f;
        var oldMin = Helper.WorldToGridPos(curPosition - halfSize, cellSize);
        var oldMax = Helper.WorldToGridPos(curPosition + halfSize, cellSize);

        var newMin = Helper.WorldToGridPos(newPosition - halfSize, cellSize);
        var newMax = Helper.WorldToGridPos(newPosition + halfSize, cellSize);

        for (int x = oldMin.x; x <= oldMax.x; x++)
        {
            for (int y = oldMin.y; y <= oldMax.y; y++)
            {
                if (x < newMin.x || x > newMax.x || y < newMin.y || y > newMax.y)
                {
                    removeGrid.Add(new int3(x, y, updateCollider.instanceId)); 
                }
            }
        }

        for (int x = newMin.x; x <= newMax.x; x++)
        {
            for (int y = newMin.y; y <= newMax.y; y++)
            {
                if (x < oldMin.x || x > oldMax.x || y < oldMin.y || y > oldMax.y)
                {
                    gridMap.Add(new int2(x, y), updateCollider.instanceId);
                }
            }
        }
    }
}
