using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootVictimA : MonoBehaviour
{
    public Transform shootPos;
    
    public class Baker : Baker<ShootVictimA>
    {
        public override void Bake(ShootVictimA authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim()
            {
                shootPos = authoring.shootPos.localPosition,
            });
        }
    }
}

public struct ShootVictim : IComponentData
{
    public float3 shootPos;
}