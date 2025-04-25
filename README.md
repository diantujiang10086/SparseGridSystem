# SparseGridSystem

`SparseGridSystem` 是一个为 Unity Job System 和 Burst 编译器设计的二维稀疏网格系统

## 特性 Features

- ✅ **Add Collider**  
  将自定义的碰撞体注册进稀疏网格系统，支持批量并发添加。

- ✅ **Remove Collider**  
  从网格中移除碰撞体，支持 Job 并发移除与缓存处理。

- ✅ **Update Collider**  
  碰撞移动时更新网格。

- ✅ **Query Radius Collider**  
  查询某一位置周围指定半径内的所有碰撞体。

- ✅ **Collision Detection**  
  碰撞检测

## 系统架构

该系统基于以下 Unity 原生集合和 Job 系统实现：

- `NativeList<T>`  
- `NativeParallelHashMap<K, V>`  
- `NativeParallelMultiHashMap<K, V>`  
- `IJobParallelFor` / `IJob`  
- 在帧结束统一处理增删改（Add/Remove/Update）操作，避免数据竞争。

## 使用场景

- 大量物体的空间索引与快速范围查询

## 示例

```csharp
// 添加一个碰撞体
public class TestCollider : BaseCollider
{
    public TestCollider(float size)
    {
        Size = size;
    }
}

// 半径查询
var result = new NativeList<int>(1024, Allocator.TempJob);
Dependency = new QueryRadiusColliderJob
{
    result = result.AsParallelWriter(),
}.Schedule(position, radius, Dependency);

// 碰撞检测
var resultList = new NativeList<CollisionDetectionEvent>(4096,Allocator.TempJob);
var job = new ColliderDetectionResult
{
    resultArray = resultList.AsParallelWriter()
}.Schedule(Dependency);

```

未来计划
 多形状碰撞检测

 