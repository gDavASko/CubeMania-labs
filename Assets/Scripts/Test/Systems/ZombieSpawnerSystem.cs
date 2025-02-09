using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct ZombieSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*var env = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer entityCommandBuffer = 
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach(var (lt, zs) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>()) 
        {
            zs.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (zs.ValueRO.timer > 0)
                continue;

            zs.ValueRW.timer = zs.ValueRO.timerMax;

            var zombie = state.EntityManager.Instantiate(env.zombieEntity);
            SystemAPI.SetComponent(zombie, LocalTransform.FromPosition(lt.ValueRO.Position ));

            entityCommandBuffer.AddComponent(zombie, new RandomWalking
            {
                originPosition = lt.ValueRO.Position,
                targetPosition = lt.ValueRO.Position,
                minDist = zs.ValueRO.minRndDist,
                maxDist = zs.ValueRO.maxRndDist,
                Random = new Unity.Mathematics.Random((uint)zombie.Index)
            });
        }*/
    }
}
