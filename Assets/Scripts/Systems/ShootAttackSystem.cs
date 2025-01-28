using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var refs = SystemAPI.GetSingleton<EntitiesReferences>();

       foreach(var (shooter, target, trs) in SystemAPI
            .Query< RefRW<ShootAttack>, 
                    RefRO<Target>,
                    RefRO<LocalTransform>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
                continue;

            if(shooter.ValueRO.timer > 0f)
            {
                shooter.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                continue;
            }

            shooter.ValueRW.timer = shooter.ValueRO.timerMax;

            var bullet = state.EntityManager.Instantiate(refs.bulletEntity);
            SystemAPI.SetComponent(bullet, LocalTransform.FromPosition(trs.ValueRO.Position));

            var bBullet = SystemAPI.GetComponentRW<Bullet>(bullet);
            bBullet.ValueRW.damage = shooter.ValueRO.damage;

            var tBullet = SystemAPI.GetComponentRW<Target>(bullet);
            tBullet.ValueRW.targetEntity = target.ValueRO.targetEntity;
        }
    }
}
