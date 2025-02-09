using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthDeadTestSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commands = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
       // var commands = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (health, entity) in SystemAPI.Query<RefRO<Health>>().WithEntityAccess())
        {
            if (health.ValueRO.health <= 0f)
            {
                commands.DestroyEntity(entity);
            }
        }

        //commands.Playback(state.EntityManager);
    }
}
