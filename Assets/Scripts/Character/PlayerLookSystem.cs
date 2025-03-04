using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace GDB.Character
{
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
}