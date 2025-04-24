using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


public struct QueryRadiusColliderEvent
{
    public int instanceId;
}

public interface IQueryRadiusColliderEventJobBase
{
    void Execute(QueryRadiusColliderEvent queryRangeColliderEvent);
}

public interface IQueryRadiusColliderEventJob: IQueryRadiusColliderEventJobBase
{

}

[BurstCompile]
internal struct QueryRadiusColliderEventJob<T> : IJobParallelFor where T : struct, IQueryRadiusColliderEventJobBase
{
    [ReadOnly] public NativeArray<int> instanceIds;
    public T jobData;
    public void Execute(int index)
    {
        jobData.Execute(new QueryRadiusColliderEvent { instanceId = instanceIds[index] });
    }
}


internal struct QueryRadiusCollider
{
    public float2 position;
    public float radius;
}

[BurstCompile]
internal struct GenerateGridCellsJob : IJob
{
    [ReadOnly] public float cellSize;
    [ReadOnly] public QueryRadiusCollider query;
    public NativeList<int2> outCells;

    public void Execute()
    {
        int2 min = Helper.WorldToGridPos(query.position - query.radius, cellSize);
        int2 max = Helper.WorldToGridPos(query.position + query.radius, cellSize);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                outCells.Add(new int2(x, y));
            }
        }
    }

    public static int CountCoveredCells(QueryRadiusCollider query, float cellSize)
    {
        int2 min = Helper.WorldToGridPos(query.position - query.radius, cellSize);
        int2 max = Helper.WorldToGridPos(query.position + query.radius, cellSize);
        int width = max.x - min.x + 1;
        int height = max.y - min.y + 1;

        return width * height;
    }
}

[BurstCompile]
internal struct QueryRadiusColliderJob : IJobParallelForDefer
{
    [ReadOnly] public float2 queryPos;
    [ReadOnly] public float queryDistSqr;
    [ReadOnly] public NativeArray<int2> cells; 
    [ReadOnly] public NativeParallelMultiHashMap<int2, int> gridMap;
    [ReadOnly] public NativeParallelHashMap<int, int> idToIndex;
    [ReadOnly] public NativeList<Collider> colliders;
    public NativeParallelHashSet<int>.ParallelWriter result;
    public void Execute(int index)
    {
        var cell = cells[index];
        if (!gridMap.TryGetFirstValue(cell, out var id, out var it))
            return;
        do
        {
            if (idToIndex.TryGetValue(id, out var colliderIndex))
            {
                var collider = colliders[colliderIndex];
                float2 colliderPos = collider.position;
                float distSqr = math.lengthsq(queryPos - colliderPos);
                if (distSqr <= queryDistSqr)
                {
                    result.Add(id);
                }
            }
        }
        while (gridMap.TryGetNextValue(out id, ref it));
    }
}