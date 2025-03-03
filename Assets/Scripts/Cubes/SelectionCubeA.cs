using Unity.Entities;
using UnityEngine;

namespace GDB.Planetary
{
    public class SelectionCubeA : MonoBehaviour
    {
        public class Baker : Baker<SelectionCubeA>
        {
            public override void Bake(SelectionCubeA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SelectionCube());
            }
        }
    }
    
    public struct SelectionCube : IComponentData{}
}