using Unity.Entities;
using UnityEngine;

namespace GDB.Planetary
{
    public class CubeA : MonoBehaviour
    {
        [SerializeField] private CubeType type;

        public class Baker: Baker<CubeA>
        {
            public override void Bake(CubeA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Cube()
                {
                    Type = authoring.type,
                });
            }
        }
    }

    public struct Cube : IComponentData
    {
        public CubeType Type;
    }
    
    public struct DestroyableCube: IComponentData, IEnableableComponent{}
}