using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile()]
internal struct RemoveColliderJob : IJob
{
    private static readonly IntComparer intComparer = default;
    private static readonly IndexComparer indexComparer = default;
    public NativeParallelHashSet<InstanceIdWithIndex> removeEntries;
    public NativeParallelHashMap<int, int> idToIndex;
    public NativeList<Collider> colliders;
    public NativeList<int> colliderDetections;
    public NativeParallelHashMap<int, int> colliderDetectionMap;
    public NativeParallelHashSet<int> removeColliderDetectionIndexs;
    public void Execute()
    {
        var removeArray = removeEntries.ToNativeArray(Allocator.Temp);
        removeArray.Sort(indexComparer);

        for (int i = 0; i < removeArray.Length; i++)
        {
            var entry = removeArray[i];
            idToIndex.Remove(entry.instanceId);

            colliderDetectionMap.Remove(entry.instanceId);
            
            int indexToRemove = entry.colliderIndex;
            int lastIndex = colliders.Length - 1;

            if (indexToRemove != lastIndex)
            {
                var moved = colliders[lastIndex];
                idToIndex[moved.instanceId] = indexToRemove;
            }
            colliders.RemoveAtSwapBack(indexToRemove);
        }

        var removeColliderDetectionIndexsArray = removeColliderDetectionIndexs.ToNativeArray(Allocator.Temp);
        removeColliderDetectionIndexsArray.Sort(intComparer);

        for (var i = 0; i < removeColliderDetectionIndexsArray.Length; i++)
        {
            var lastIndex = colliderDetections.Length - 1;
            int removeIndex = removeColliderDetectionIndexsArray[i];

            if(removeIndex != lastIndex)
            {
                var instanceId = colliderDetections[lastIndex];
                colliderDetectionMap[instanceId] = removeIndex;
            }

            colliderDetections.RemoveAtSwapBack(removeIndex);
        }

        removeArray.Dispose();
        removeColliderDetectionIndexsArray.Dispose();
    }

    struct IndexComparer : IComparer<InstanceIdWithIndex>
    {
        public int Compare(InstanceIdWithIndex x, InstanceIdWithIndex y)
        {
            return y.colliderIndex.CompareTo(x.colliderIndex);
        }
    }

    struct IntComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return y.CompareTo(x);
        }
    }
}
