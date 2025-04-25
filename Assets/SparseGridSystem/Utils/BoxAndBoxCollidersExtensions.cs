using System.Runtime.CompilerServices;
using Unity.Mathematics;

/// <summary>
/// GPT生成未测试
/// </summary>
internal static class BoxAndBoxCollidersExtensions
{
    public struct CollisionAxis
    {
        public float2 axis;
        public float2 p0, p1, p2, p3;

        public CollisionAxis(float2 p0, float2 p1, float2 p2, float2 p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.axis = math.normalize(p1 - p0);  // 计算轴
        }

        public bool IsSeparatingAxis(float2 otherP0, float2 otherP1, float2 otherP2, float2 otherP3)
        {
            Project4(p0, p1, p2, p3, axis, out float minA, out float maxA);
            Project4(otherP0, otherP1, otherP2, otherP3, axis, out float minB, out float maxB);
            return maxA < minB || maxB < minA;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Overlap(in this BoxCollider a, in BoxCollider b)
    {
        float2 aHalf = a.header.size * 0.5f;
        float2 bHalf = b.header.size * 0.5f;

        float2x2 rotA = float2x2.Rotate(a.header.angle);
        float2x2 rotB = float2x2.Rotate(b.header.angle);

        // 获取四个角点位置
        float2 a0 = a.header.position + math.mul(rotA, new float2(-aHalf.x, -aHalf.y));
        float2 a1 = a.header.position + math.mul(rotA, new float2(aHalf.x, -aHalf.y));
        float2 a2 = a.header.position + math.mul(rotA, new float2(aHalf.x, aHalf.y));
        float2 a3 = a.header.position + math.mul(rotA, new float2(-aHalf.x, aHalf.y));

        float2 b0 = b.header.position + math.mul(rotB, new float2(-bHalf.x, -bHalf.y));
        float2 b1 = b.header.position + math.mul(rotB, new float2(bHalf.x, -bHalf.y));
        float2 b2 = b.header.position + math.mul(rotB, new float2(bHalf.x, bHalf.y));
        float2 b3 = b.header.position + math.mul(rotB, new float2(-bHalf.x, bHalf.y));

        // 定义 4 个碰撞轴
        var axisA = new CollisionAxis(a0, a1, a2, a3);
        var axisB = new CollisionAxis(b0, b1, b2, b3);

        // 检查每个轴是否存在间隙
        if (axisA.IsSeparatingAxis(b0, b1, b2, b3)) return false;  // a1 - a0
        if (axisA.IsSeparatingAxis(b0, b1, b2, b3)) return false;  // a3 - a0
        if (axisB.IsSeparatingAxis(a0, a1, a2, a3)) return false;  // b1 - b0
        if (axisB.IsSeparatingAxis(a0, a1, a2, a3)) return false;  // b3 - b0

        return true;
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

