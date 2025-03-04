using Unity.Entities;
using UnityEngine;

namespace GDB.Planetary
{
    public class GravityTargetA : MonoBehaviour
    {
        public class Baker : Baker<GravityTargetA>
        {
            public override void Bake(GravityTargetA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GravityTarget());
            }
        }
    }
    
    public struct GravityTarget : IComponentData, IEnableableComponent{}
}