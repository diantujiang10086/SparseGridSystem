using Unity.Mathematics;
using UnityEngine;

public class QueryRadiusComponent : MonoBehaviour
{
    public Color color = Color.red;
    public float radius = 10;

    public float2 position
    {
        get
        {
            return new float2(transform.position.x, transform.position.y);
        }
    }
    private void Awake()
    {
        transform.position = new Vector3(50, 50, 0);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.white;
    }
}