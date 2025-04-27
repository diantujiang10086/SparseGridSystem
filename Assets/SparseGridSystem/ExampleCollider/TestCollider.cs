using UnityEngine;

public class TestCollider : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        var drawColliders = Transform.FindObjectsOfType<DrawCollider>();
        foreach (var drawCollider in drawColliders)
        {
            drawCollider.UpdateCollider();
        }

        foreach (var collider in drawColliders)
        {
            bool isCollider = false;
            foreach (var collider2 in drawColliders)
            {
                if (collider == collider2)
                    continue;
                isCollider |= collider.Overlap(collider2);
            }
            collider.Draw(isCollider);
        }
    }
}