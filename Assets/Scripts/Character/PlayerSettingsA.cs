using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    public class PlayerSettingsA : MonoBehaviour
    {
        [field: SerializeField] public float MoveSpeed { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; }
        [field: SerializeField] public float LookSensitivity { get; private set; }
        
        [field: SerializeField] public bool UseGravity { get; private set; }
        
        public class Baker : Baker<PlayerSettingsA>
        {

            public override void Bake(PlayerSettingsA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerSettings
                {
                    MoveSpeed = authoring.MoveSpeed,
                    JumpForce = authoring.JumpForce,
                    LookSensitivity = authoring.LookSensitivity,
                    IsFirstPerson = true,
                    UseGravity = authoring.UseGravity,
                });
            }
            
        }
    }
    
    public struct PlayerSettings : IComponentData
    {
        public float MoveSpeed; // Скорость движения
        public float LookSensitivity; // Чувствительность камеры
        public float JumpForce; // Сила прыжка
        public bool IsFirstPerson; // Текущий вид камеры
        public bool UseGravity;
    }
}