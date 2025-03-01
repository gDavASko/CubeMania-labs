using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    public class PlayerControlA : MonoBehaviour
    {
        public class Baker : Baker<PlayerControlA>
        {
            public override void Bake(PlayerControlA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerControl());
            }
        }
    }

    public struct PlayerControl : IComponentData, IEnableableComponent
    {
        
    }
}