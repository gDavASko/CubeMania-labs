using Unity.Entities;
using UnityEngine;

namespace GDB.Common
{
    public class CameraTagA : MonoBehaviour
    {
        public class Baker : Baker<CameraTagA>
        {
            public override void Bake(CameraTagA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CameraTag());
            }
        }
    }
    
    public struct CameraTag : IComponentData
    {
    }
}