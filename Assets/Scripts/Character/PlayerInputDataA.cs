using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GDB.Character
{
    public class PlayerInputDataA : MonoBehaviour
    {
        public class Baker : Baker<PlayerInputDataA>
        {
            public override void Bake(PlayerInputDataA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlayerInputData());
            }
        }
    }
    
    public struct PlayerInputData : IComponentData
    {
        public float2 Move; // Вектор движения (WASD)
        public float2 Look; // Вектор вращения камеры (мышь)
        public bool Jump; // Прыжок
        public bool Sit; // Прыжок
        public bool CamChangeNeedProcess;
    }
}