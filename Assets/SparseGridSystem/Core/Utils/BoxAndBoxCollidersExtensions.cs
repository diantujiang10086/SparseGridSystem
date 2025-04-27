using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SparseGrid
{
    public static class BoxAndBoxCollidersExtensions
    {
        public struct CollisionAxis
        {
            public float2 axis0; // p0 -> p1
            public float2 axis1; // p0 -> p3
            public float2 p0, p1, p2, p3;

            public CollisionAxis(float2 p0, float2 p1, float2 p2, float2 p3)
            {
                this.p0 = p0;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.axis0 = math.normalize(p1 - p0);
                this.axis1 = math.normalize(p3 - p0);
            }

            public bool IsSeparatingAxis(float2 otherP0, float2 otherP1, float2 otherP2, float2 otherP3, float2 axis)
            {
                Project4(p0, p1, p2, p3, axis, out float minA, out float maxA);
                Project4(otherP0, otherP1, otherP2, otherP3, axis, out float minB, out float maxB);
                return maxA < minB || maxB < minA;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap(in this BoxCollider boxA, in BoxCollider boxB)
        {
            CalculateBoxCorners(boxA, out var aCorner0, out var aCorner1, out var aCorner2, out var aCorner3);
            CalculateBoxCorners(boxB, out var bCorner0, out var bCorner1, out var bCorner2, out var bCorner3);

            // 为每个盒子定义碰撞轴
            var axesA = new CollisionAxis(aCorner0, aCorner1, aCorner2, aCorner3);
            var axesB = new CollisionAxis(bCorner0, bCorner1, bCorner2, bCorner3);

            // 用 A 的两条边
            if (axesA.IsSeparatingAxis(bCorner0, bCorner1, bCorner2, bCorner3, axesA.axis0)) return false;
            if (axesA.IsSeparatingAxis(bCorner0, bCorner1, bCorner2, bCorner3, axesA.axis1)) return false;

            // 用 B 的两条边
            if (axesB.IsSeparatingAxis(aCorner0, aCorner1, aCorner2, aCorner3, axesB.axis0)) return false;
            if (axesB.IsSeparatingAxis(aCorner0, aCorner1, aCorner2, aCorner3, axesB.axis1)) return false;

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateBoxCorners(in BoxCollider box, out float2 corner0, out float2 corner1, out float2 corner2, out float2 corner3)
        {
            float2 pos = box.header.position + box.center;
            float2 halfSize = box.size * 0.5f;

            float angle = box.header.angle;
            float cos = math.cos(angle);
            float sin = math.sin(angle);

            float2 right = new float2(cos, sin);
            float2 up = new float2(-sin, cos);

            float2 rightOffset = right * halfSize.x;
            float2 upOffset = up * halfSize.y;

            corner0 = pos - rightOffset - upOffset; // 左下
            corner1 = pos + rightOffset - upOffset; // 右下
            corner2 = pos + rightOffset + upOffset; // 右上
            corner3 = pos - rightOffset + upOffset; // 左上
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Project4(
            float2 p0, float2 p1, float2 p2, float2 p3, float2 axis,
            out float min, out float max)
        {
            float p0Proj = math.dot(p0, axis);
            float p1Proj = math.dot(p1, axis);
            float p2Proj = math.dot(p2, axis);
            float p3Proj = math.dot(p3, axis);

            min = math.min(math.min(p0Proj, p1Proj), math.min(p2Proj, p3Proj));
            max = math.max(math.max(p0Proj, p1Proj), math.max(p2Proj, p3Proj));
        }
    }
}