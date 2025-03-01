using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

namespace GDB.Character
{
    partial struct PlayerMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;

            foreach (var (input, characterState, 
                         transform, physVel) in
                     SystemAPI.Query<RefRO<PlayerInputData>,
                         RefRO<PlayerSettings>,
                         RefRW<LocalTransform>,
                         RefRW<PhysicsVelocity>>())
            {
                float3 forward = math.mul(transform.ValueRO.Rotation, new float3(0, 0, 1)); // Направление вперед
                float3 right = math.mul(transform.ValueRO.Rotation, new float3(1, 0, 0)); // Направление вправо

                float3 movement = forward * input.ValueRO.Move.y + right * input.ValueRO.Move.x;
                movement.y = 0;

                // Применяем скорость к Rigidbody
                physVel.ValueRW.Linear.xz = movement.xz * characterState.ValueRO.MoveSpeed * SystemAPI.Time.DeltaTime;
            }
        }
    }

    partial struct PlayerJumpSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;

            foreach (var (input, characterState, 
                         physVel, mass) in
                     SystemAPI.Query<RefRO<PlayerInputData>,
                         RefRO<PlayerSettings>,
                         RefRW<PhysicsVelocity>,
                         RefRO<PhysicsMass>>())
            {
                if (input.ValueRO.Jump && physVel.ValueRO.Linear.y <= 0.01f)
                {
                    physVel.ValueRW.Linear.y = characterState.ValueRO.JumpForce * mass.ValueRO.InverseMass;
                }
            }
        }
    }
    
    public partial struct PlayerLookSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.ManagedAPI.GetSingleton<PlayerLink>();
            
            foreach (var (input, lookData, transform, settings) in
                     SystemAPI
                         .Query<RefRO<PlayerInputData>, RefRW<PlayerLookData>, RefRW<LocalTransform>,
                             RefRO<PlayerSettings>>())
            {
                float2 lookInput = input.ValueRO.Look * settings.ValueRO.LookSensitivity;

                // Обновляем угол поворота персонажа
                lookData.ValueRW.Rotation.x -= lookInput.y; // Вверх/вниз (по Y)
                lookData.ValueRW.Rotation.y += lookInput.x; // Влево/вправо (по X)

                // Ограничение наклона вверх/вниз (от -89 до 89 градусов)
                lookData.ValueRW.Rotation.x =
                    math.clamp(lookData.ValueRW.Rotation.x, -math.radians(89), math.radians(89));

                // Обновляем поворот
                quaternion xRotation = quaternion.Euler(lookData.ValueRO.Rotation.x, 0, 0); // Вертикальный наклон
                quaternion yRotation = quaternion.Euler(0, lookData.ValueRO.Rotation.y, 0); // Горизонтальный поворот

                transform.ValueRW.Rotation = math.mul(yRotation, xRotation);
                
                // Применяем вращение через angularVelocity
                //player.Rigidbody.MoveRotation( math.mul(yRotation, xRotation));
            }
        }
    }

    public partial class GravitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            
            foreach (var (player, phys, mass, entity) in SystemAPI
                         .Query<RefRW<PlayerControl>, RefRW<PhysicsVelocity>, RefRO<PhysicsMass>>().WithEntityAccess())
            {
                var idx = physicsWorld.GetRigidBodyIndex(entity);
                physicsWorld.ApplyLinearImpulse(idx, math.up() * -9.81f / mass.ValueRO.InverseMass + phys.ValueRW.Linear);
                
                //phys.ValueRW.Linear += new float3(0, -9.81f / mass.ValueRO.InverseMass, 0);
            }
        }
    }
}