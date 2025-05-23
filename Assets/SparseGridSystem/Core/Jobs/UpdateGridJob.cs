﻿using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace SparseGrid
{
    internal struct UpdateColliderInfo
    {
        public Collider collider;
        public float2 newPosition;
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    internal struct PrepareUpdateColliderInfoJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<UpdateCollider>.ReadOnly updateArray;
        [ReadOnly] public NativeParallelHashMap<int, int> idToIndex;
        [ReadOnly] public NativeList<Collider> colliders;

        [WriteOnly] public NativeArray<UpdateColliderInfo> output;

        public void Execute(int index)
        {
            var update = updateArray[index];
            if (idToIndex.TryGetValue(update.instanceId, out var colliderIndex))
            {
                output[index] = new UpdateColliderInfo
                {
                    collider = colliders[colliderIndex],
                    newPosition = update.position
                };
            }
            else
            {
                output[index] = default;
            }
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    internal struct UpdateGridJob : IJobParallelFor
    {
        [ReadOnly] public float cellSize;
        [ReadOnly] public NativeArray<UpdateColliderInfo> updateColliderInfos;
        public NativeParallelMultiHashMap<int2, int>.ParallelWriter gridMap;
        public NativeParallelHashSet<int3>.ParallelWriter removeGrid;
        public void Execute(int index)
        {
            var info = updateColliderInfos[index];
            var collider = info.collider;
            int instanceId = collider.header.instanceId;
            if (instanceId == 0)
                return;

            var newPosition = info.newPosition;
            var newRectCollider = Helper.CalculateRectCollider(collider, newPosition, cellSize);
            if (collider.rectCollider == newRectCollider)
                return;

            var oldMin = collider.rectCollider.min;
            var oldMax = collider.rectCollider.max;
            var newMin = newRectCollider.min;
            var newMax = newRectCollider.max;

            for (int x = oldMin.x; x <= oldMax.x; x++)
            {
                for (int y = oldMin.y; y <= oldMax.y; y++)
                {
                    if (x < newMin.x || x > newMax.x || y < newMin.y || y > newMax.y)
                    {
                        removeGrid.Add(new int3(x, y, instanceId));
                    }
                }
            }

            for (int x = newMin.x; x <= newMax.x; x++)
            {
                for (int y = newMin.y; y <= newMax.y; y++)
                {
                    if (x < oldMin.x || x > oldMax.x || y < oldMin.y || y > oldMax.y)
                    {
                        gridMap.Add(new int2(x, y), instanceId);
                    }
                }
            }
        }
    }
}