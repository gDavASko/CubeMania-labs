using Unity.Entities;
using UnityEngine;

namespace GDB.Common
{
    public class SelectableTagA : MonoBehaviour
    {
        public class Baker : Baker<SelectableTagA>
        {

            public override void Bake(SelectableTagA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SelectableTag());
            }
        }
    }
    
    public struct SelectableTag : IComponentData {}
    
}