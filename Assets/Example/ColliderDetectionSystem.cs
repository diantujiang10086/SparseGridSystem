using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Logging;
using Unity.Mathematics;
using UnityEngine;

public struct ColliderDetectionResult : ICollisionDetectionEventJob
{
    public NativeList<CollisionDetectionEvent>.ParallelWriter resultArray;
    public void Execute(CollisionDetectionEvent collisionDetectionEvent)
    {
        resultArray.AddNoResize(collisionDetectionEvent);
    }
}

public partial class ColliderDetectionSystem : SystemBase
{
    private Example example;
    private HashSet<int> previousCollidedSet = new HashSet<int>(4096);

    protected override void OnCreate()
    {
        example = Transform.FindAnyObjectByType<Example>();
    }
    protected override void OnUpdate()
    {
        var resultList = new NativeList<CollisionDetectionEvent>(4096,Allocator.TempJob);
        var job = new ColliderDetectionResult
        {
            resultArray = resultList.AsParallelWriter()
        }.Schedule(Dependency);
        job.Complete();
        Job.WithCode(() => 
        {
            var currentCollidedSet = new HashSet<int>();

            for (int i = 0; i < resultList.Length; i++)
            {
                var result = resultList[i];

                if (example.dict.TryGetValue(result.instanceIdA, out var unitA))
                {
                    unitA.SetColor(Color.red);
                    currentCollidedSet.Add(result.instanceIdA);
                }

                if (example.dict.TryGetValue(result.instanceIdB, out var unitB))
                {
                    unitB.SetColor(Color.red);
                    currentCollidedSet.Add(result.instanceIdB);
                }
            }

            foreach (var id in previousCollidedSet)
            {
                if (!currentCollidedSet.Contains(id) && example.dict.TryGetValue(id, out var unit))
                {
                    unit.SetColor(Color.white);
                }
            }

            previousCollidedSet = currentCollidedSet;

        }).WithoutBurst().WithDisposeOnCompletion(resultList).Run();
    }
}

