using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MoveOverrideA : MonoBehaviour
{
    
    public class Baker : Baker<MoveOverrideA>
    {
        public override void Bake(MoveOverrideA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new MoveOverride());
            SetComponentEnabled<MoveOverride>(e, false);
        }
    }
}

public struct MoveOverride: IComponentData, IEnableableComponent
{
    public float3 targetPosition;
}