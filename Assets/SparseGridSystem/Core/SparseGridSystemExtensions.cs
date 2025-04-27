using Unity.Jobs;
using Unity.Entities;
using Unity.Mathematics;
namespace SparseGrid
{
    public static class SparseGridSystemExtensions
    {
        private static SparseGridSystem system;
        private static SparseGridSystem System
        {
            get
            {
                if (system == null)
                {
                    system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SparseGridSystem>();
                }
                return system;
            }
        }
        internal static void Clear()
        {
            system = null;
        }
        internal static void AddCollider(this ICollider collider)
        {
            System.AddCollider(collider);
        }

        internal static void RemoveCollider(this ICollider collider)
        {
            System.RemoveCollider(collider);
        }

        internal static void UpdateCollider(this ICollider collider)
        {
            System.UpdateCollider(collider);
        }

        public static JobHandle Schedule<T>(this T jobData, float2 position, float radius, JobHandle inputDeps) where T : struct, IQueryRadiusColliderEventJobBase
        {
            return System.QueryRadius(new QueryRadiusCollider { position = position, radius = radius }, jobData, inputDeps);
        }

        public static JobHandle Schedule<T>(this T jobData, JobHandle inputDeps) where T : struct, ICollisionDetectionEventJobBase
        {
            return System.QueryColliderDetection(jobData, inputDeps);
        }
    }
}