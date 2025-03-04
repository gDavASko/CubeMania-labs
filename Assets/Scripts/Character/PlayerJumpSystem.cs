using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace GDB.Character
{
    partial struct PlayerJumpSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;

            foreach (var (input, settings, 
                         physVel, mass, trs) in
                     SystemAPI.Query<RefRO<PlayerInputData>,
                         RefRO<PlayerSettings>,
                         RefRW<PhysicsVelocity>,
                         RefRO<PhysicsMass>,
                         RefRW<LocalTransform>>())
            {
                if (settings.ValueRO.UseGravity)
                {
                    if (input.ValueRO.Jump && physVel.ValueRO.Linear.y <= 0.01f)
                    {
                        physVel.ValueRW.Linear.y = settings.ValueRO.JumpForce * mass.ValueRO.InverseMass;
                    }
                }
                else
                {
                    if (input.ValueRO.JumpUngravity)
                    {
                        physVel.ValueRW.Linear.y = 1f;
                        
                    }
                    else if (input.ValueRO.Sit)
                    {
                        physVel.ValueRW.Linear.y = -1f;
                    }
                    else
                    {
                        physVel.ValueRW.Linear.y = 0f;
                    }
                }
                
            }
        }
    }
}