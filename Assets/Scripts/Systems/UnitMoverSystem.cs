using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob job = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);

        /*foreach(var (lt, um, physVel) in
            SystemAPI.Query<
                RefRW<LocalTransform>, 
                RefRO<UnitMover>,
                RefRW<PhysicsVelocity>>())
        {

            float3 dir = um.ValueRO.targetPos - lt.ValueRO.Position;
            dir = math.normalize(dir);

            lt.ValueRW.Rotation = 
                math.slerp(lt.ValueRW.Rotation, 
                    quaternion.LookRotation(dir, math.up()), 
                    SystemAPI.Time.DeltaTime * um.ValueRO.rotSpeed);
               

            physVel.ValueRW.Linear = dir * um.ValueRO.moveSpeed;
            physVel.ValueRW.Angular = float3.zero;

            //lt.ValueRW.Position += dir * spd.ValueRO.value * SystemAPI.Time.DeltaTime;
        }*/
    }
}

[BurstCompile]
public partial struct UnitMoverJob: IJobEntity
{
    public float deltaTime;

    public void Execute(ref LocalTransform lt, in UnitMover um, ref PhysicsVelocity physVel)
    {
        float3 dir = um.targetPos - lt.Position;

        float targetDist = 2f;
        if (math.lengthsq(dir) < targetDist)
        {
            physVel.Linear = float3.zero;
            physVel.Angular = float3.zero;
            return;
        }

        dir = math.normalize(dir);

        lt.Rotation =
            math.slerp(lt.Rotation,
                quaternion.LookRotation(dir, math.up()),
                deltaTime * um.rotSpeed);

        physVel.Linear = dir * um.moveSpeed;
        physVel.Angular = float3.zero;
    }
}
