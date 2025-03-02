using Unity.Entities;
using UnityEngine;

namespace GDB.Planetary
{
    public class BaseCubeA : MonoBehaviour
    {

        public class Baker : Baker<BaseCubeA>
        {
            public override void Bake(BaseCubeA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BaseCube());
            }
        }
    }

    public struct BaseCube : IComponentData, IEnableableComponent
    { }
}