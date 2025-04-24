using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class QueryColliderSystem : SystemBase
{
    private Example test;
    private QueryRadiusComponent queryRadiusComponent;
    private HashSet<Unit> currentUnits;
    private HashSet<Unit> lastUnits;

    protected override void OnCreate()
    {
        currentUnits = new HashSet<Unit>(4096);
        lastUnits = new HashSet<Unit>(4096);
        test = Transform.FindAnyObjectByType<Example>();
        queryRadiusComponent = new GameObject().AddComponent<QueryRadiusComponent>();
    }

    protected override void OnUpdate()
    {
        var result = new NativeList<int>(4096 * 4, Allocator.TempJob);
        Dependency = new QueryRadiusColliderJob
        {
            result = result.AsParallelWriter(),
        }.Schedule(queryRadiusComponent.position, queryRadiusComponent.radius, Dependency);

        Job.WithCode(() =>
        {
            currentUnits.Clear();

            foreach (var instance in result)
            {
                if (test.dict.TryGetValue(instance, out var unit))
                {
                    currentUnits.Add(unit);
                    unit.SetColor(Color.red);
                }
            }

            foreach (var unit in lastUnits)
            {
                if (!currentUnits.Contains(unit))
                {
                    unit.SetColor(Color.white);
                }
            }

            (lastUnits, currentUnits) = (currentUnits, lastUnits);

        }).WithoutBurst().WithDisposeOnCompletion(result).Run();
    }
}
