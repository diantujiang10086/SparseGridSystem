using SparseGrid;
using UnityEngine;

public class DrawCollider : MonoBehaviour 
{
    protected new SparseGrid.Collider collider;
    public virtual void Draw(bool isCollider)
    {
        Gizmos.color = isCollider ? Color.red : Color.white;
        OnDraw();
        Gizmos.color = Color.white;
    }


    public virtual void UpdateCollider()
    {

    }

    protected virtual void OnDraw()
    {

    }

    public bool Overlap(DrawCollider drawCollider)
    {
        return CollisionHelper.Overlap(ref collider, ref drawCollider.collider);
    }
}
