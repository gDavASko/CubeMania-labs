using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverA : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float rotSpeed = 10f;

    public class Baker : Baker<UnitMoverA>
    {
        public override void Bake(UnitMoverA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new UnitMover 
            { 
                moveSpeed = authoring.moveSpeed, 
                rotSpeed = authoring.rotSpeed,
            });
        }
    }
}

public struct UnitMover : IComponentData
{
    public float moveSpeed;
    public float rotSpeed;
    public float3 targetPos;

}
