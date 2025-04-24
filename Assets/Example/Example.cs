using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Example : MonoBehaviour
{
    public bool isDrawGrid = false;
    public bool isUpdate = true;
    public int count = 4000;
    public int size = 1;
    public GameObject prefab;
    public int instanceId = 0;
    public int colliderLength;
    public int idToIndexLength;
    public List<Unit> units = new List<Unit>();
    public readonly Dictionary<int, Unit> dict = new Dictionary<int, Unit>(1000);
    private Unity.Mathematics.Random random;
    private void Awake()
    {
        random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SparseGridSystem>().Initialize(count, size);
        for (int i = 0; i < count; i++)
        {
            AddCollider();
        }
    }
    private void Update()
    {
        if (isUpdate)
        {
            var value = random.NextFloat(0, 100);
            if (value < 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    AddCollider();
                }
            }
            else if (value > 90)
            {
                for (int i = 0; i < 10; i++)
                {
                    RemoveCollider(random.NextInt(0, units.Count));
                }
            }
        }

        for (int i = 0; i < units.Count; i++)
        {
            units[i].Update();
        }
    }
    private void RemoveCollider(int index)
    {
        var unit = units[index];
        dict.Remove(unit.InstanceId);
        units.RemoveAt(index);
        unit.Dispose();
    }
    private void AddCollider()
    {
        var instance = GameObject.Instantiate(prefab);
        var unit = new Unit(++instanceId,instance.transform);
        unit.Position = random.NextFloat2(0, 100);
        unit.SetCollider(new TestCollider(1));
        unit.SetMoveComponent(new MoveComponent());
        dict[unit.InstanceId] = unit;
        units.Add(unit);
    }
}
