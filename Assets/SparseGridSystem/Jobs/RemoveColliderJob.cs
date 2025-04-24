using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System.Collections.Generic;

[BurstCompile()]
internal struct RemoveColliderJob : IJob
{
    private static readonly IndexComparer defaultComparer = default;
    public NativeParallelHashSet<InstanceIdWithIndex> removeEntries;
    public NativeParallelHashMap<int, int> idToIndex;
    public NativeList<Collider> colliders;
    public void Execute()
    {
        var array = removeEntries.ToNativeArray(Allocator.Temp);
        array.Sort(defaultComparer);

        for (int i = 0; i < array.Length; i++)
        {
            var entry = array[i];
            idToIndex.Remove(entry.instanceId);

            int indexToRemove = entry.colliderIndex;
            int lastIndex = colliders.Length - 1;

            if (indexToRemove != lastIndex)
            {
                var moved = colliders[lastIndex];
                idToIndex[moved.instanceId] = indexToRemove;
            }
            colliders.RemoveAtSwapBack(indexToRemove);
        }
        array.Dispose();
    }

    struct IndexComparer : IComparer<InstanceIdWithIndex>
    {
        public int Compare(InstanceIdWithIndex x, InstanceIdWithIndex y)
        {
            return y.colliderIndex.CompareTo(x.colliderIndex);
        }
    }
}
