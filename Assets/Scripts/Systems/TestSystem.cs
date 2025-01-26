using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct TestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {/*
        int unitCount = 0;

        foreach (var (lt, um, physVel) in
            SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRO<UnitMover>,
                RefRW<PhysicsVelocity>>()
                .WithPresent<Selected>())
        {

            unitCount ++;
        }*/
    }
}
