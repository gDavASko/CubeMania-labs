using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    public class PlayerLinkA : MonoBehaviour
    {
        public class Baker : Baker<PlayerLinkA>
        {
            public override void Bake(PlayerLinkA authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                var Link = new PlayerLink(){};
                AddComponentObject(e, Link);
            }
        }
    }
    
    public class PlayerLink : IComponentData
    {
        public GameObject Link;
    }
}