using Unity.Burst;
using Unity.Entities;

partial struct ShootLightDestroySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach(var (light, entity) in SystemAPI.Query<RefRW<ShootLight>>().WithEntityAccess())
        {

            light.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (light.ValueRO.timer > 0f) 
                continue;

            buffer.DestroyEntity(entity);
        }
    }
}
