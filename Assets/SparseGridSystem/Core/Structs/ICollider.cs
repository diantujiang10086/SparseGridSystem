﻿using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SparseGrid
{
    public interface ICollider
    {
        int InstanceId { get; }
        bool IsEnableColliderDetection { get; }
        float2 Size { get; }
        float2 Position { get; }
        float angle { get; }
        ColliderShape ColliderShape { get; }
        int Layer { get; }
        int ColliderLayer { get; }
        /// <summary>
        /// Colliders of different types at the same hierarchy level
        /// </summary>
        int ColliderType { get; }
        int ColliderColliderType { get; }

        IGeometry Geometry { get; }
    }

    public interface ColliderEvent
    {
        void OnColliderEnter();
    }

    public abstract class BaseCollider : ICollider, IDisposable
    {
        private float2 position;
        private IGeometry geometry;
        public int InstanceId { get; set; }

        public float2 Size { get; set; }

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

        public float angle { get; set; }

        public IGeometry Geometry
        {
            get => geometry;
            set
            {
                geometry = value;
            }
        }


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
}