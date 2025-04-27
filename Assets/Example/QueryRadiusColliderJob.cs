using Unity.Collections;
using SparseGrid;
public struct QueryRadiusColliderJob : IQueryRadiusColliderEventJob
{
    public NativeList<int>.ParallelWriter result;
    public void Execute(QueryRadiusColliderEvent queryRangeColliderEvent)
    {
        result.AddNoResize(queryRangeColliderEvent.instanceId);
    }
}