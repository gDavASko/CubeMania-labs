using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct FindTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var phys = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var cols = phys.CollisionWorld;
        var colFilter = new CollisionFilter()
                            {
                                GroupIndex = 0,
                                BelongsTo = ~0u,
                                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                            };

        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);

        foreach(var (trs, finder, target) in SystemAPI
            .Query<RefRO<LocalTransform>, 
                    RefRW<FindTarget>,
                    RefRW<Target>>())
        {
            if(finder.ValueRO.timer > 0)
            {
                finder.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                continue;
            }

            finder.ValueRW.timer = finder.ValueRO.timerMax;

            hits.Clear();
            if (cols.OverlapSphere(trs.ValueRO.Position, finder.ValueRO.range, ref hits, colFilter))
            {
                foreach (var hit in hits)
                {
                    if(SystemAPI.GetComponent<Unit>(hit.Entity).fraction == finder.ValueRO.targetFraction)
                    {
                        target.ValueRW.targetEntity = hit.Entity;
                        break;
                    }
                }

            }

        }
    }
}
