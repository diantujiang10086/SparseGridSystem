using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class CircleDrawCollider : DrawCollider
{
    public float radius;

    protected override void OnDraw()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public override void UpdateCollider()
    {
        var position = transform.position;
        var circleCollider = new SparseGrid.CircleCollider { header = new SparseGrid.ColliderHeader { colliderShape = SparseGrid.ColliderShape.Circle }, center = new float2(position.x, position.y), radius = radius };
        collider = UnsafeUtility.As<SparseGrid.CircleCollider, SparseGrid.Collider>(ref circleCollider);
    }
}
