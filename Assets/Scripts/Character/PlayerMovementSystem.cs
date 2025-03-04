using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace GDB.Character
{
    partial struct PlayerMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (input, settings, 
                         transform, physVel) in
                     SystemAPI.Query<RefRO<PlayerInputData>,
                         RefRO<PlayerSettings>,
                         RefRW<LocalTransform>,
                         RefRW<PhysicsVelocity>>())
            {
                float3 forward = math.mul(transform.ValueRO.Rotation, new float3(0, 0, 1)); // Направление вперед
                float3 right = math.mul(transform.ValueRO.Rotation, new float3(1, 0, 0)); // Направление вправо

                float3 movement = forward * input.ValueRO.Move.y + right * input.ValueRO.Move.x;

                if (settings.ValueRO.UseGravity)
                {
                    physVel.ValueRW.Linear.xz = movement.xz * settings.ValueRO.MoveSpeed * SystemAPI.Time.DeltaTime;
                }
                else
                {
                    physVel.ValueRW.Linear = movement * settings.ValueRO.MoveSpeed * SystemAPI.Time.DeltaTime;
                }
            }
        }
    }
}