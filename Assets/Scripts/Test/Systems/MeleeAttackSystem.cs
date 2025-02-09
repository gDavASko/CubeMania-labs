using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var colls = physicsWorld.CollisionWorld;
        NativeList<RaycastHit> hits = new NativeList<RaycastHit>(Allocator.Temp);

        foreach (var (mAtacker, target, trs, mover) in SystemAPI
            .Query<RefRW<MeleeAttack>,
                    RefRO<Target>,
                    RefRW<LocalTransform>,
                    RefRW<UnitMover>>()
                    .WithDisabled<MoveOverride>())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
                continue;

            var targetTrs = state.EntityManager.GetComponentData<LocalTransform>(target.ValueRO.targetEntity);

            bool isNear = math.distance(trs.ValueRO.Position, targetTrs.Position) <= 2f;

            bool isTouch = false;
            if (!isNear)
            {
                float3 dirCol = targetTrs.Position - trs.ValueRO.Position;
                dirCol = math.normalize(dirCol);
                float extraDist = 0.4f;

                var raycast = new RaycastInput
                {
                    Start = trs.ValueRO.Position,
                    End = trs.ValueRO.Position + dirCol * (mAtacker.ValueRO.colliderSize + extraDist),
                    Filter = CollisionFilter.Default
                };

                hits.Clear();
                if (colls.CastRay(raycast, ref hits))
                {
                    foreach (var hit in hits)
                    {
                        if (hit.Entity == target.ValueRO.targetEntity)
                        {
                            isTouch = true;
                            break;
                        }
                    }
                }
            }


            if (!isNear && !isTouch)
            {
                UnityEngine.Debug.LogError("Is Far!!");
                mover.ValueRW.targetPos = targetTrs.Position;
                continue;
            }
            else
            {
                mover.ValueRW.targetPos = trs.ValueRO.Position;
            }

            mAtacker.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (mAtacker.ValueRO.timer > 0f)
            {                
                continue;
            }

            float3 dir = targetTrs.Position - trs.ValueRO.Position;
            dir = math.normalize(dir);

            trs.ValueRW.Rotation =
                math.slerp(trs.ValueRW.Rotation,
                            quaternion.LookRotation(dir, math.up()),
                            SystemAPI.Time.DeltaTime * mover.ValueRO.rotSpeed);

            

            mAtacker.ValueRW.timer = mAtacker.ValueRO.timerMax;

            var health = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
            health.ValueRW.health -= mAtacker.ValueRO.damage;
            health.ValueRW.onHPChanged = true;
        }
    }
}
