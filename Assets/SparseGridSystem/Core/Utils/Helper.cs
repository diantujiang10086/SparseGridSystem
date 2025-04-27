using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SparseGrid
{
    internal static class Helper
    {
        public static T As<T>(this IGeometry self) where T : IGeometry
        {
            return (T)self;
        }

        public static Collider CreateCollider(this ICollider collider)
        {
            var geometry = collider.Geometry;
            switch (collider.ColliderShape)
            {
                case ColliderShape.Box:
                    {
                        var boxGeometry = collider.Geometry.As<BoxGeometry>();
                        var boxCollider = new BoxCollider
                        {
                            header = collider.CreateColliderHeader(),
                            center = boxGeometry.center,
                            size = boxGeometry.size
                        };
                        return UnsafeUtility.As<BoxCollider, Collider>(ref boxCollider);
                    }
                case ColliderShape.Circle:
                    {
                        var circleGeometry = collider.Geometry.As<CircleGeometry>();
                        var circleCollider = new CircleCollider
                        {
                            header = collider.CreateColliderHeader(),
                            center = circleGeometry.center,
                            radius = circleGeometry.radius
                        };
                        return UnsafeUtility.As<CircleCollider, Collider>(ref circleCollider);
                    }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ColliderHeader CreateColliderHeader<T>(this T collider) where T : ICollider
        {
            return new ColliderHeader
            {
                instanceId = collider.InstanceId,
                position = collider.Position,
                size = collider.Size,
                angle = collider.angle,
                layer = collider.Layer,
                colliderLayer = collider.ColliderLayer,
                colliderType = collider.ColliderType,
                colliderColliderType = collider.ColliderColliderType,
                isEnableColliderDetection = collider.IsEnableColliderDetection ? 1 : 0,
                colliderShape = collider.ColliderShape
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldToGridPos(float2 worldPos, float cellSize)
        {
            return (int2)math.floor(worldPos / cellSize);
        }

        public static RectCollider CalculateRectCollider(Collider collider, float2 position, float cellSize)
        {
            switch (collider.header.colliderShape) 
            {
                case ColliderShape.Box:
                    {
                        ref readonly var boxCollider = ref UnsafeUtility.As<Collider, BoxCollider>(ref collider);
                        GetGrid(position + boxCollider.center, boxCollider.size, cellSize, out var min, out var max);
                        return new RectCollider(min, max);
                    }
                case ColliderShape.Circle:
                    {
                        ref readonly var circleCollider = ref UnsafeUtility.As<Collider, CircleCollider>(ref collider);
                        GetGrid(position + circleCollider.center, circleCollider.radius, cellSize, out var min, out var max);
                        return new RectCollider(min, max);
                    }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetGrid(float2 position, float2 size, float cellSize, out int2 min, out int2 max)
        {
            var halfSize = size * 0.5f;
            min = Helper.WorldToGridPos(position - halfSize, cellSize);
            max = Helper.WorldToGridPos(position + halfSize, cellSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnableColliderDetection(this Collider collider)
        {
            return collider.header.isEnableColliderDetection == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCheckCollideType(this Collider collider)
        {
            return collider.header.colliderColliderType != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanCollideWith(this Collider collider, in Collider other)
        {
            return (collider.header.colliderLayer & other.header.layer) != 0 &&
                   (other.header.colliderLayer & collider.header.layer) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanCollideTypeWith(this Collider collider, in Collider other)
        {
            return (collider.header.colliderColliderType & other.header.colliderType) != 0 &&
                   (other.header.colliderColliderType & collider.header.colliderType) != 0;
        }
    }
}