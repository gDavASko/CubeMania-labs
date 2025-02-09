using Unity.Entities;
using UnityEngine;

namespace GDB.Planetary
{
    public class SysDataA : MonoBehaviour
    {
        public class Baker: Baker<SysDataA>
        {
            public override void Bake(SysDataA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SysData());
            }
        }
    }

    public struct SysData: IComponentData, IEnableableComponent
    {

    }
}