using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootLightSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*var refs = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach (var shooter in SystemAPI.Query<RefRW<ShootAttack>>())
        {
            if (shooter.ValueRO.onShoot.IsTriggered)
            {
                var light = state.EntityManager.Instantiate(refs.shootLightEntity);
                SystemAPI.SetComponent<LocalTransform>
                    (light, LocalTransform.FromPosition(shooter.ValueRO.onShoot.ShootPos));
            }
        }*/
    }
}
