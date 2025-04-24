using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

public enum ColliderShape: int
{
    None = 0,
    Box = 1,
    Circle = 2,
    Polygon = 3,
}

public interface ICollider
{
    int InstanceId { get; }
    bool IsEnableColliderDetection { get; }
    float2 Size { get; }
    float2 Position { get; }
    ColliderShape ColliderShape { get; }
    int Layer { get; }
    int ColliderLayer { get; }
    /// <summary>
    /// Colliders of different types at the same hierarchy level
    /// </summary>
    int ColliderType { get; }
    int ColliderColliderType { get; }
}

public abstract class BaseCollider : ICollider, IDisposable
{
    private float2 position;
    public int InstanceId { get; set; }

    public float2 Size { get;  set; }

    public float2 Position 
    {
        get
        {
            return position;
        }
        set
        {
            position = value;
            this.UpdateCollider();
        }
    }

    public int Layer { get; set; }

    public int ColliderLayer { get; private set; }

    public int ColliderType { get; set; }

    public int ColliderColliderType { get; private set; }

    public ColliderShape ColliderShape { get; set; } = ColliderShape.Box;

    public bool IsEnableColliderDetection { get; set; }

    public void Initialize(float2 position)
    {
        this.position = position;
        this.AddCollider();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCollisionLayer(int layer)
    {
        ColliderLayer |= 1 << layer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveCollisionLayer(int layer)
    {
        ColliderLayer &= ~(1 << layer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCollisionCollisionType(int layer)
    {
        ColliderColliderType |= 1 << layer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveCollisionCollisionType(int layer)
    {
        ColliderColliderType &= ~(1 << layer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearColliderLayer()
    {
        ColliderLayer = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearCollisionCollisionType()
    {
        ColliderColliderType = 0;
    }


    public void Dispose()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            return;
#endif
        this.RemoveCollider();
        OnDispose();
    }

    protected virtual void OnDispose()
    {

    }
}