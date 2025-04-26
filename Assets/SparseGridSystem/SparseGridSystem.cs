using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(LateSimulationSystemGroup))]
public partial class SparseGridSystem : SystemBase
{
    private const int defaultCapacity = 4096;
    private float cellSize = 1;
    private DoubleBufferList<int> removeBuffer;
    private DoubleBufferList<Collider> addBuffer;
    private DoubleBufferList<UpdateCollider> updateBuffer;

    private NativeList<int> colliderDetections;
    private NativeParallelHashMap<int, int> colliderDetectionMap;
    private NativeList<Collider> colliders;
    private NativeParallelHashMap<int, int> idToIndex;
    private NativeParallelMultiHashMap<int2, int> gridMap;

    private NativeParallelHashSet<int2> colliderDetectionResultSet;
    private NativeList<int2> colliderDetectionResultArray;
    private JobHandle collisionDetectionHandle;

    public void Initialize(int capacity, int cellSize)
    {
        this.cellSize = cellSize;
        int initCapacity = math.max(defaultCapacity, capacity);
        

        if(removeBuffer == null)
        {
            removeBuffer = new DoubleBufferList<int>(initCapacity, Allocator.Persistent);
            addBuffer = new DoubleBufferList<Collider>(initCapacity, Allocator.Persistent);
            updateBuffer = new DoubleBufferList<UpdateCollider>(initCapacity, Allocator.Persistent);

            colliderDetections = new NativeList<int>(initCapacity, Allocator.Persistent);
            colliderDetectionMap = new NativeParallelHashMap<int, int>(initCapacity, Allocator.Persistent);
            colliders = new NativeList<Collider>(initCapacity, Allocator.Persistent);
            idToIndex = new NativeParallelHashMap<int, int>(initCapacity, Allocator.Persistent);
            gridMap = new NativeParallelMultiHashMap<int2, int>(initCapacity * 10, Allocator.Persistent);
            colliderDetectionResultSet = new NativeParallelHashSet<int2>(initCapacity * 2, Allocator.Persistent);
            colliderDetectionResultArray = new NativeList<int2>(initCapacity * 2, Allocator.Persistent);
        }
        else
        {
            colliderDetections.Capacity = initCapacity;
            colliderDetectionMap.Capacity = initCapacity;
            removeBuffer.Capacity = initCapacity;
            addBuffer.Capacity = initCapacity;
            updateBuffer.Capacity = initCapacity;
            colliders.Capacity = initCapacity;
            idToIndex.Capacity = initCapacity;
            gridMap.Capacity = initCapacity * 10;
            colliderDetectionResultSet.Capacity = initCapacity * 2;
            colliderDetectionResultArray.Capacity = initCapacity * 2;
        }
    }

    internal void AddCollider(ICollider collider)
    {
        addBuffer.Add(collider.CreateCollider());
    }

    internal void RemoveCollider(ICollider collider)
    {
        removeBuffer.Add(collider.InstanceId);
    }

    internal void UpdateCollider(ICollider collider) 
    {
        updateBuffer.Add(new UpdateCollider { instanceId = collider.InstanceId, position = collider.Position });
    }

    internal JobHandle QueryRadius<T>(QueryRadiusCollider queryRangeCollider, T jobData, JobHandle inputDeps) where T : struct, IQueryRadiusColliderEventJobBase
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

    internal JobHandle QueryColliderDetection<T>(T jobData, JobHandle inputDeps) where T : struct, ICollisionDetectionEventJobBase
    {
        var combinedDeps = JobHandle.CombineDependencies(Dependency, inputDeps);
        var jobHandle = new CollisionDetectionEventJob<T>
        {
            instanceIds = colliderDetectionResultArray.AsDeferredJobArray(),
            jobData = jobData
        }.Schedule(colliderDetectionResultArray, 128, combinedDeps);
        return JobHandle.CombineDependencies(Dependency, jobHandle);
    }

    protected override void OnCreate()
    {
        var _default = SparseGridSetting.Default;
        Initialize(_default.capacity, _default.cellSize);
    }

    protected override void OnDestroy()
    {
        colliderDetectionResultSet.Dispose();
        colliderDetectionResultArray.Dispose();
        addBuffer.Dispose();
        removeBuffer.Dispose();
        updateBuffer.Dispose();

        idToIndex.Dispose();
        colliderDetections.Dispose();
        colliderDetectionMap.Dispose();
        colliders.Dispose();
        gridMap.Dispose();
        SparseGridSystemExtensions.Clear();
    }

    protected override void OnUpdate()
    {
        Dependency = RemoveColliders(Dependency);
        Dependency = AddColliders(Dependency);
        Dependency = UpdateColliders(Dependency);
        Dependency = ColliderDetection(Dependency);

    }

    JobHandle RemoveColliders(JobHandle inputDeps)
    {
        var removeInstanceIdArray = removeBuffer.Update();
        if (removeInstanceIdArray.Length == 0)
            return inputDeps;

        var removeColliderIndexsSet = new NativeParallelHashSet<InstanceIdWithIndex>(removeInstanceIdArray.Length, Allocator.TempJob);
        var removeGridInstanceIdsSet = new NativeParallelHashSet<int3>(removeInstanceIdArray.Length * 4, Allocator.TempJob);
        var removeColliderDetectionIndexs = new NativeParallelHashSet<int>(removeInstanceIdArray.Length, Allocator.TempJob);
        var handle = new RemoveIndexsJob
        {
            cellSize = cellSize,
            colliders = colliders,
            idToIndex = idToIndex,
            removeArray = removeInstanceIdArray,
            colliderDetectionMap = colliderDetectionMap,
            removeGridInstanceIds = removeGridInstanceIdsSet.AsParallelWriter(),
            removeColliderIndexs = removeColliderIndexsSet.AsParallelWriter(),
            removeColliderDetectionIndexs = removeColliderDetectionIndexs.AsParallelWriter(),
        }.Schedule(removeInstanceIdArray.Length, 128, inputDeps);
        handle = new RemoveColliderJob
        {
            idToIndex = idToIndex,
            colliders = colliders,
            removeEntries = removeColliderIndexsSet,
            colliderDetections = colliderDetections,
            colliderDetectionMap = colliderDetectionMap,
            removeColliderDetectionIndexs = removeColliderDetectionIndexs,
        }.Schedule(handle);
        handle = new RemoveGridMapJob
        {
            gridMap = gridMap,
            removeGridInstanceIds = removeGridInstanceIdsSet
        }.Schedule(handle);
        inputDeps = handle;
        var disposeHandle1 = removeColliderIndexsSet.Dispose(handle);
        var disposeHandle2 = removeGridInstanceIdsSet.Dispose(handle);
        inputDeps = JobHandle.CombineDependencies(disposeHandle1, disposeHandle2);
        return inputDeps;
    }

    JobHandle AddColliders(JobHandle inputDeps)
    {
        var addArray = addBuffer.Update();
        if (addArray.Length <= 0)
            return inputDeps;

        inputDeps.Complete();
        int startLength = colliders.Length;
        colliders.ResizeUninitialized(colliders.Length + addArray.Length);
        NativeList<int> addColliderDetections = new NativeList<int>(addArray.Length, Allocator.Persistent);
        var handle = new AddColliderJob
        {
            cellSize = cellSize,
            arrayLength = startLength,
            addArray = addArray,
            colliders = colliders,
            gridMap = gridMap.AsParallelWriter(),
            idToIndex = idToIndex.AsParallelWriter(),
            addColliderDetections = addColliderDetections.AsParallelWriter(),
        }.Schedule(addArray.Length, 128, inputDeps);
        handle.Complete();

        int colliderArrayLength = colliderDetections.Length;
        colliderDetections.ResizeUninitialized(colliderDetections.Length + addColliderDetections.Length);
        var handle2 = new AddColliderDetectionJob
        {
            colliderArrayLength = colliderArrayLength,
            addColliderDetections = addColliderDetections,
            colliderDetections = colliderDetections,
            colliderDetectionMap = colliderDetectionMap.AsParallelWriter(),
        }.Schedule(addColliderDetections, 128, handle);
        addColliderDetections.Dispose(handle2);
        inputDeps = handle2;
        return inputDeps;
    }

    JobHandle UpdateColliders(JobHandle inputDeps)
    {
        var updateArray = updateBuffer.Update();
        if (updateArray.Length == 0)
            return inputDeps;

        var removeGridInstanceIdsSet = new NativeParallelHashSet<int3>(updateArray.Length * 4, Allocator.TempJob);
        var handle = new UpdateGridJob
        {
            cellSize = cellSize,
            gridMap = gridMap.AsParallelWriter(),
            colliders = colliders,
            idToIndex = idToIndex,
            removeGrid = removeGridInstanceIdsSet.AsParallelWriter(),
            updateColliders = updateArray
        }.Schedule(updateArray.Length, 128, inputDeps);
        handle = new RemoveGridMapJob
        {
            gridMap = gridMap,
            removeGridInstanceIds = removeGridInstanceIdsSet
        }.Schedule(handle);
        removeGridInstanceIdsSet.Dispose(handle);
        inputDeps = new UpdateColliderJob
        {
            colliders = colliders,
            idToIndex = idToIndex,
            updateColliders = updateArray
        }.Schedule(updateArray.Length, 128, handle);

        return inputDeps;
    }

    JobHandle ColliderDetection(JobHandle inputDeps)
    {
        if (collisionDetectionHandle.IsCompleted)
        {
            colliderDetectionResultSet.Clear();
            colliderDetectionResultArray.Clear();
        }
        collisionDetectionHandle = new CollisionDetectionJob
        {
            cellSize = cellSize,
            gridMap = gridMap,
            idToIndex = idToIndex,
            colliderDetections = colliderDetections,
            colliders = colliders.AsDeferredJobArray(),
            result = colliderDetectionResultSet.AsParallelWriter(),
            resultArray = colliderDetectionResultArray.AsParallelWriter()
        }.Schedule(colliderDetections, 128, inputDeps);
        return collisionDetectionHandle;
    }

}
