using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    public class PlayersettingsA : MonoBehaviour
    {
        [field: SerializeField] public float MoveSpeed { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; }
        [field: SerializeField] public float LookSensitivity { get; private set; }
        
        public class Baker : Baker<PlayersettingsA>
        {

            public override void Bake(PlayersettingsA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerSettings
                {
                    MoveSpeed = authoring.MoveSpeed,
                    JumpForce = authoring.JumpForce,
                    LookSensitivity = authoring.LookSensitivity
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
    }
}