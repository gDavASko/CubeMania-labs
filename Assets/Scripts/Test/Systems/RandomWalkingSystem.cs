using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (um, rw, lt) in 
            SystemAPI.Query<RefRW<UnitMover>, RefRW<RandomWalking>, RefRO<LocalTransform>>())
        {
            if(math.distancesq(lt.ValueRO.Position, rw.ValueRO.targetPosition) <= UnitMoverSystem.REACH_TARGET_DISTANCE)
            {
                var rnd = rw.ValueRO.Random;

                float3 dir = new float3(rnd.NextFloat(-1f, 1f), 0, rnd.NextFloat(-1f, 1f));
                dir = math.normalize(dir);

                rw.ValueRW.targetPosition = rw.ValueRO.originPosition + 
                    dir * rnd.NextFloat(rw.ValueRO.minDist, rw.ValueRO.maxDist);

                rw.ValueRW.Random = rnd;
            }
            else
            {
                um.ValueRW.targetPos = rw.ValueRO.targetPosition;
            }
        }
    }
}
