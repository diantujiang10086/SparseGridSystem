using System;
using Unity.Mathematics;

public interface ICollider
{
    int InstanceId { get; }
    float2 Size { get; }
    float2 Position { get; }
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

    public void Initialize(float2 position)
    {
        this.position = position;
        this.AddCollider();
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