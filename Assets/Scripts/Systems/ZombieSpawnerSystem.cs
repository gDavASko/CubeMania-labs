using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct ZombieSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var env = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach(var (lt, zs) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>()) 
        {
            zs.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (zs.ValueRO.timer > 0)
                continue;

            zs.ValueRW.timer = zs.ValueRO.timerMax;

            var zombie = state.EntityManager.Instantiate(env.zombieEntity);
            SystemAPI.SetComponent(zombie, LocalTransform.FromPosition(lt.ValueRO.Position ));
        }
    }
}
