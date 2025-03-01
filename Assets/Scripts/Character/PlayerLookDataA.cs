using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GDB.Character
{
    public class PlayerLookDataA : MonoBehaviour
    {
        public class Baker : Baker<PlayerLookDataA>
        {
            public override void Bake(PlayerLookDataA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerLookData());
            }
        }
    }
    
    public struct PlayerLookData : IComponentData
    {
        public float2 Rotation; // Углы поворота (X - горизонталь, Y - вертикаль)
    }
}