using Unity.Jobs;

public class TestCollider : BaseCollider
{
    public TestCollider(float size, bool isEnableColliderDetection)
    {
        ColliderShape = ColliderShape.Box;
        IsEnableColliderDetection = isEnableColliderDetection;
        Layer = 1;
        AddCollisionLayer(0);
        Size = size;
    }
}
