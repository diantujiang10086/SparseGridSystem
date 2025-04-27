using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace SparseGrid
{
    [BurstCompile()]
    internal struct UpdateColliderJob : IJobParallelFor
    {
        public float cellSize;
        [ReadOnly] public NativeArray<UpdateCollider>.ReadOnly updateColliders;
        [ReadOnly] public NativeParallelHashMap<int, int> idToIndex;
        [NativeDisableParallelForRestriction] public NativeList<Collider> colliders;
        public void Execute(int index)
        {
            var updateCollider = updateColliders[index];
            if (!idToIndex.TryGetValue(updateCollider.instanceId, out var colliderIndex))
                return;

            Collider collider = colliders[colliderIndex];
            collider.header.position = updateCollider.position;
            collider.rectCollider = Helper.CalculateRectCollider(collider, updateCollider.position, cellSize);
            colliders[colliderIndex] = collider;
        }
    }
}