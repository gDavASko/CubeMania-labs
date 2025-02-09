using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MoveOverrideSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (lt, mo, emo, um) in SystemAPI
            .Query<RefRO<LocalTransform>, RefRO<MoveOverride>, EnabledRefRW<MoveOverride>, RefRW<UnitMover>>())
        {
            if(math.distancesq(lt.ValueRO.Position, mo.ValueRO.targetPosition) > UnitMoverSystem.REACH_TARGET_DISTANCE)
            {
                um.ValueRW.targetPos = mo.ValueRO.targetPosition;
            }
            else
            {
                emo.ValueRW = false;
            }
        }
    }
}
