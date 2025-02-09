using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct InitUnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        foreach(var (lt, um, ium, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<UnitMover>, RefRO<InitUnitMover>>()
            .WithEntityAccess())
        {            
            um.ValueRW.targetPos = lt.ValueRO.Position;
            buffer.RemoveComponent<InitUnitMover>(entity);
        }
    }
}
