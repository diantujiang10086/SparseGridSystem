using SparseGrid;
using Unity.Jobs;

public class TestCollider : BaseCollider
{
    public TestCollider(float size, bool isEnableColliderDetection)
    {
        ColliderShape = ColliderShape.Box;
        Geometry = new BoxGeometry { center = 0, size = size };
        IsEnableColliderDetection = isEnableColliderDetection;
        Layer = 1;
        AddCollisionLayer(0);
        Size = size;
    }
}
