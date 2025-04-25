using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public struct CollisionDetectionEvent
{
    public int instanceIdA;
    public int instanceIdB;
}

public interface ICollisionDetectionEventJobBase
{
    void Execute(CollisionDetectionEvent collisionDetectionEvent);
}

public interface ICollisionDetectionEventJob : ICollisionDetectionEventJobBase
{

}

[BurstCompile]
internal struct CollisionDetectionEventJob<T> : IJobParallelForDefer where T : struct, ICollisionDetectionEventJobBase
{
    [ReadOnly] public NativeArray<int2> instanceIds;
    public T jobData;
    public void Execute(int index)
    {
        var instanceId = instanceIds[index];
        jobData.Execute(new CollisionDetectionEvent {  instanceIdA = instanceId.x, instanceIdB = instanceId.y });
    }
}

[BurstCompile()]
internal struct CollisionDetectionJob : IJobParallelForDefer
{
    [ReadOnly] public float cellSize;
    [ReadOnly] public NativeList<int> colliderDetections;
    [ReadOnly] public NativeArray<Collider> colliders;
    [ReadOnly] public NativeParallelHashMap<int, int> idToIndex;
    [ReadOnly] public NativeParallelMultiHashMap<int2, int> gridMap;
    public NativeParallelHashSet<int2>.ParallelWriter result;
    public NativeList<int2>.ParallelWriter resultArray;
    public void Execute(int index)
    {
        var colliderDetectionInstanceId = colliderDetections[index];
        if (!idToIndex.TryGetValue(colliderDetectionInstanceId, out var colliderIndex))
            return;

        var collider = colliders[colliderIndex];
        collider.GetGrid(cellSize, out var min, out var max);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Foreach(in collider, new int2(x, y));
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Foreach(in Collider collider, int2 cell)
    {
        if (!gridMap.TryGetFirstValue(cell, out var id, out var it))
            return;
        do
        {
            if (idToIndex.TryGetValue(id, out var colliderIndex))
            {
                var otherCollider = colliders[colliderIndex];
                if (collider != otherCollider && collider.CanCollideWith(otherCollider))
                {
                    bool isCheckColliderType = collider.IsCheckCollideType();
                    bool value = !isCheckColliderType || (isCheckColliderType && collider.CanCollideTypeWith(otherCollider));
                    if(value)
                    {
                        AddResult(in collider, in otherCollider);
                    }
                }
            }
        } while (gridMap.TryGetNextValue(out id, ref it));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void AddResult(in Collider collider, in Collider otherCollider)
    {
        if (!CollisionHelper.Overlap(in collider, in otherCollider))
            return;

        int2 pair = collider.instanceId < otherCollider.instanceId
            ? new int2(collider.instanceId, otherCollider.instanceId)
            : new int2(otherCollider.instanceId, collider.instanceId);

        if (result.Add(pair))
        {
            resultArray.AddNoResize(pair);
        }
    }
}
