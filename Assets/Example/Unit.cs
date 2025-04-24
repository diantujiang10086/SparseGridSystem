using System;
using Unity.Mathematics;

using UnityEngine;

public class Unit : IDisposable
{
    public int InstanceId { get; private set; }
    private float2 position;
    private Transform transform;
    private BaseCollider colliderComponent;
    private MoveComponent moveComponent;
    private MeshRenderer meshRenderer;
    public float2 Position
    {
        get
        {
            return position;
        }
        set
        {
            position = value;
            if (colliderComponent != null)
            {
                colliderComponent.Position = position;
            }
            transform.position = new Vector3(position.x, position.y, 0);
        }
    }

    public Unit(int instanceId, Transform transform)
    {
        this.InstanceId = instanceId;
        this.transform = transform;
        meshRenderer = transform.GetComponent<MeshRenderer>();
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    public void SetMoveComponent(MoveComponent moveComponent)
    {
        this.moveComponent = moveComponent;
        moveComponent.Start(this);
    }

    public void SetCollider(BaseCollider baseCollider)
    {
        baseCollider.InstanceId = InstanceId;
        this.colliderComponent = baseCollider;
        baseCollider.Initialize(position);
    }

    public void Update()
    {
        moveComponent.Update();
    }

    public void Dispose()
    {
        if (transform != null)
        {
            GameObject.Destroy(transform.gameObject);
        }
    }
}