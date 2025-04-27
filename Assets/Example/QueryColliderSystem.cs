using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using SparseGrid;

public partial class QueryColliderSystem : SystemBase
{
    private Example example;
    private QueryRadiusComponent queryRadiusComponent;
    private HashSet<Unit> currentUnits;
    private HashSet<Unit> lastUnits;

    protected override void OnCreate()
    {
        currentUnits = new HashSet<Unit>(4096);
        lastUnits = new HashSet<Unit>(4096);
        example = Transform.FindAnyObjectByType<Example>();
        if (example != null)
        {
            queryRadiusComponent = new GameObject().AddComponent<QueryRadiusComponent>();
        }
    }

    protected override void OnUpdate()
    {
        if (example == null)
            return;
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
                if (example.dict.TryGetValue(instance, out var unit))
                {
                    currentUnits.Add(unit);
                    unit.SetColor(Color.green);
                }
            }

            foreach (var unit in lastUnits)
            {
                if (!currentUnits.Contains(unit))
                {
                    if(!unit.IsDisposable)
                        unit.SetColor(Color.white);
                }
            }

            (lastUnits, currentUnits) = (currentUnits, lastUnits);

        }).WithoutBurst().WithDisposeOnCompletion(result).Run();
    }
}

