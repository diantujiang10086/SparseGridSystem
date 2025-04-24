using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(LateSimulationSystemGroup))]
public partial class SparseGridSystem : SystemBase
{
    private int defaultCapacity = 4096;
    private int defaultCellSize = 1;
    private int initCapacity = 4096;
    private float cellSize = 1;
    private DoubleBufferList<int> removeBuffer;
    private DoubleBufferList<Collider> addBuffer;
    private DoubleBufferList<UpdateCollider> updateBuffer;
    private DoubleBufferList<QueryRadiusCollider> queryRangeColliderBuffer;
    private NativeList<Collider> colliders;
    private NativeParallelHashMap<int, int> idToIndex;
    private NativeParallelMultiHashMap<int2, int> gridMap;

    private NativeParallelMultiHashMap<int, int> queryRadiusColliders;

    public void Initialize(int capacity, int cellSize)
    {
        this.cellSize = cellSize;
        initCapacity = math.max(defaultCapacity, capacity);

        if(queryRangeColliderBuffer == null)
        {
            queryRangeColliderBuffer = new DoubleBufferList<QueryRadiusCollider>(initCapacity, Allocator.Persistent);
            queryRadiusColliders = new NativeParallelMultiHashMap<int, int>(initCapacity, Allocator.Persistent);
            removeBuffer = new DoubleBufferList<int>(initCapacity, Allocator.Persistent);
            addBuffer = new DoubleBufferList<Collider>(initCapacity, Allocator.Persistent);
            updateBuffer = new DoubleBufferList<UpdateCollider>(initCapacity, Allocator.Persistent);

            colliders = new NativeList<Collider>(initCapacity, Allocator.Persistent);
            idToIndex = new NativeParallelHashMap<int, int>(initCapacity, Allocator.Persistent);
            gridMap = new NativeParallelMultiHashMap<int2, int>(initCapacity * 10, Allocator.Persistent);
        }
        else
        {
            queryRangeColliderBuffer.Capacity = initCapacity;
            queryRadiusColliders.Capacity = initCapacity;
            removeBuffer.Capacity = initCapacity;
            addBuffer.Capacity = initCapacity;
            updateBuffer.Capacity = initCapacity;
            colliders.Capacity = initCapacity;
            idToIndex.Capacity = initCapacity;
            gridMap.Capacity = initCapacity * 10;
        }
    }

    internal void AddCollider(ICollider collider)
    {
        addBuffer.Add(new Collider 
        { 
            instanceId = collider.InstanceId, 
            position = collider.Position, 
            size = collider.Size 
        });
    }

    internal void RemoveCollider(ICollider collider)
    {
        removeBuffer.Add(collider.InstanceId);
    }

    internal void UpdateCollider(ICollider collider) 
    {
        updateBuffer.Add(new UpdateCollider { instanceId = collider.InstanceId, position = collider.Position });
    }

    internal JobHandle QueryRadius<T>(QueryRadiusCollider queryRangeCollider, T jobData, JobHandle jobHandle) where T : struct, IQueryRadiusColliderEventJobBase
    {
        var cells = new NativeList<int2>(GenerateGridCellsJob.CountCoveredCells(queryRangeCollider, cellSize), Allocator.TempJob);
        var handle =  new GenerateGridCellsJob
        {
            cellSize = cellSize,
            query = queryRangeCollider,
            outCells = cells
        }.Schedule(Dependency);
        var set = new NativeParallelHashSet<int>(4096, Allocator.TempJob);
        var handle2 = new QueryRadiusColliderJob 
        { 
            queryPos = queryRangeCollider.position,
            queryDistSqr = queryRangeCollider.radius * queryRangeCollider.radius,
            cells = cells.AsDeferredJobArray(),
            gridMap = gridMap,
            idToIndex = idToIndex,
            colliders = colliders,
            result = set.AsParallelWriter()
        }.Schedule(cells, 128, handle);
        cells.Dispose(handle2);
        handle2.Complete();
        var setArray = set.ToNativeArray(Allocator.TempJob);
        var handle3 = new QueryRadiusColliderEventJob<T>
        {
            instanceIds = setArray,
            jobData = jobData
        }.Schedule(setArray.Length, 128, handle2);
        var dispose1 = set.Dispose(handle3);
        var dispose2 = setArray.Dispose(handle3);
        return JobHandle.CombineDependencies(dispose1, dispose2);
    }

    protected override void OnCreate()
    {
        Initialize(defaultCapacity, defaultCellSize);
    }

    protected override void OnDestroy()
    {
        queryRangeColliderBuffer.Dispose();
        queryRadiusColliders.Dispose();
        addBuffer.Dispose();
        removeBuffer.Dispose();
        updateBuffer.Dispose();

        idToIndex.Dispose();
        colliders.Dispose();
        gridMap.Dispose();
        SparseGridSystemExtensions.Clear();
    }

    protected override void OnUpdate()
    {
        var addArray = addBuffer.Update();
        var removeInstanceIdArray = removeBuffer.Update();
        var updateArray = updateBuffer.Update();

        if(removeInstanceIdArray.Length > 0)
        {
            var removeColliderIndexsSet = new NativeParallelHashSet<InstanceIdWithIndex>(removeInstanceIdArray.Length, Allocator.TempJob);
            var removeGridInstanceIdsSet = new NativeParallelHashSet<int3>(removeInstanceIdArray.Length * 4, Allocator.TempJob);
            var handle = new RemoveIndexsJob
            {
                cellSize = cellSize,
                colliders = colliders,
                idToIndex = idToIndex,
                removeArray = removeInstanceIdArray,
                removeGridInstanceIds = removeGridInstanceIdsSet.AsParallelWriter(),
                removeColliderIndexs = removeColliderIndexsSet.AsParallelWriter(),
            }.Schedule(removeInstanceIdArray.Length, 128, Dependency);
            handle = new RemoveColliderJob
            {
                idToIndex = idToIndex,
                colliders = colliders,
                removeEntries = removeColliderIndexsSet,
            }.Schedule(handle);
            handle = new RemoveGridMapJob
            {
                gridMap = gridMap,
                removeGridInstanceIds = removeGridInstanceIdsSet
            }.Schedule(handle);
            Dependency = handle;
            var disposeHandle1 = removeColliderIndexsSet.Dispose(handle);
            var disposeHandle2 = removeGridInstanceIdsSet.Dispose(handle);
            Dependency = JobHandle.CombineDependencies(disposeHandle1, disposeHandle2);
        }

        if(addArray.Length > 0)
        {
            Dependency.Complete();
            int startLength = colliders.Length;
            colliders.ResizeUninitialized(colliders.Length + addArray.Length);
            Dependency = new AddColliderJob
            {
                cellSize = cellSize,
                arrayLength = startLength,
                addArray = addArray,
                colliders = colliders,
                gridMap = gridMap.AsParallelWriter(),
                idToIndex = idToIndex.AsParallelWriter(),
            }.Schedule(addArray.Length, 128, Dependency);
        }
        
        if(updateArray.Length > 0)
        {
            var removeGridInstanceIdsSet = new NativeParallelHashSet<int3>(updateArray.Length * 4, Allocator.TempJob);
            var handle = new UpdateGridJob
            {
                cellSize = cellSize,
                gridMap = gridMap.AsParallelWriter(),
                colliders = colliders,
                idToIndex = idToIndex,
                removeGrid = removeGridInstanceIdsSet.AsParallelWriter(),
                updateColliders = updateArray
            }.Schedule(updateArray.Length, 128, Dependency);
            handle = new RemoveGridMapJob
            {
                gridMap = gridMap,
                removeGridInstanceIds = removeGridInstanceIdsSet
            }.Schedule(handle);
            removeGridInstanceIdsSet.Dispose(handle);
            Dependency = new UpdateColliderJob
            {
                 colliders = colliders,
                idToIndex = idToIndex,
                 updateColliders = updateArray
            }.Schedule(updateArray.Length, 128, handle);

        }

    }
    
}
