using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (lt, bullet, target, bulletE) in SystemAPI
            .Query<
                RefRW<LocalTransform>,
                RefRO<Bullet>,
                RefRO<Target>>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                buffer.DestroyEntity(bulletE);
                continue;
            }

            var targetTrs = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.targetEntity);
            var victim = SystemAPI.GetComponentRO<ShootVictim>(target.ValueRO.targetEntity);
            var targetPoint = targetTrs.ValueRO.TransformPoint(victim.ValueRO.shootPos);

            float distBefore = math.distancesq(lt.ValueRO.Position, targetPoint);

            float3 dir = targetPoint - lt.ValueRO.Position;
            dir = math.normalize(dir);

            lt.ValueRW.Position += dir * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            float distAfter = math.distancesq(lt.ValueRO.Position, targetPoint);

            if(distBefore < distAfter)
            {
                lt.ValueRW.Position = targetPoint;
            }

            float destroyDist = 0.1f;

            if(math.distancesq(lt.ValueRO.Position, targetPoint) < destroyDist)
            {
                var targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.health -= bullet.ValueRO.damage;

                buffer.DestroyEntity(bulletE);
            }
        }
    }
}
