using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

namespace SparseGrid
{
    [BurstCompile()]
    internal struct RemoveGridMapJob : IJob
    {
        [ReadOnly] public NativeParallelHashSet<int3> removeGridInstanceIds;
        public NativeParallelMultiHashMap<int2, int> gridMap;
        public void Execute()
        {
            var array = removeGridInstanceIds.ToNativeArray(Allocator.Temp);
            foreach (var item in array)
            {
                gridMap.Remove(item.xy, item.z);
            }
            array.Dispose();
        }
    }
}