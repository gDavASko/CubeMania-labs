using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct ResetTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var target in SystemAPI
             .Query<RefRW<Target>>())
        {
            if(target.ValueRO.Value == Entity.Null) 
                continue;

            if(!SystemAPI.Exists(target.ValueRO.Value)
                || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value))
                target.ValueRW.Value = Entity.Null;
        }
    }
}
