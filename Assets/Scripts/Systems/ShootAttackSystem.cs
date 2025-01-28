using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var refs = SystemAPI.GetSingleton<EntitiesReferences>();

       foreach(var (shooter, target, trs, mover) in SystemAPI
            .Query< RefRW<ShootAttack>, 
                    RefRO<Target>,
                    RefRW<LocalTransform>,
                    RefRW<UnitMover>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null)            
                continue;

            var targetTrs = state.EntityManager.GetComponentData<LocalTransform>(target.ValueRO.targetEntity);
            if (math.distance(trs.ValueRO.Position, targetTrs.Position) > shooter.ValueRO.attackDist)
            {
                mover.ValueRW.targetPos = targetTrs.Position;
                continue;
            }
            else
            {
                mover.ValueRW.targetPos = trs.ValueRO.Position;
            }

            float3 dir = targetTrs.Position - trs.ValueRO.Position;
            dir = math.normalize(dir);

            trs.ValueRW.Rotation = 
                math.slerp(trs.ValueRW.Rotation, 
                            quaternion.LookRotation(dir, math.up()),
                            SystemAPI.Time.DeltaTime * mover.ValueRO.rotSpeed);

            if (shooter.ValueRO.timer > 0f)
            {
                shooter.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                continue;
            }

            shooter.ValueRW.timer = shooter.ValueRO.timerMax;           

            var bullet = state.EntityManager.Instantiate(refs.bulletEntity);
            var pos = trs.ValueRO.TransformPoint(shooter.ValueRO.bulletSpawnPoint);
            SystemAPI.SetComponent(bullet, LocalTransform.FromPosition(pos));

            var bBullet = SystemAPI.GetComponentRW<Bullet>(bullet);
            bBullet.ValueRW.damage = shooter.ValueRO.damage;

            var tBullet = SystemAPI.GetComponentRW<Target>(bullet);
            tBullet.ValueRW.targetEntity = target.ValueRO.targetEntity;
        }
    }
}
