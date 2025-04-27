using SparseGrid;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class BoxDrawCollider : DrawCollider
{
    public float2 size;
    protected override void OnDraw()
    {
        ref readonly var boxCollider = ref UnsafeUtility.As<SparseGrid.Collider, SparseGrid.BoxCollider>(ref collider);
        BoxAndBoxCollidersExtensions.CalculateBoxCorners(boxCollider, out var corners0, out var corners1,out var corners2,out var corners3);

        Vector2[] corners = new Vector2[4];
        corners[0] = corners0;
        corners[1] = corners1;
        corners[2] = corners2;
        corners[3] = corners3;

        for (int i = 0; i < 4; i++)
        {
            var pos = new Vector3(corners[i].x, corners[i].y, 0);
            Gizmos.DrawLine(pos, new Vector3(corners[(i + 1) % 4].x, corners[(i + 1) % 4].y, 0));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(pos, $"{pos}");
#endif
        }
    }

    public override void UpdateCollider()
    {
        var position = transform.position;
        var angle = math.radians(transform.localEulerAngles.z);
        var boxCollider = new SparseGrid.BoxCollider 
        { 
            header = new SparseGrid.ColliderHeader 
            {
                colliderShape = SparseGrid.ColliderShape.Box,
                angle = angle
            }, 
            center = new float2(position.x, position.y), 
            size = size 
        };
        collider = UnsafeUtility.As<SparseGrid.BoxCollider, SparseGrid.Collider>(ref boxCollider);
    }
}
