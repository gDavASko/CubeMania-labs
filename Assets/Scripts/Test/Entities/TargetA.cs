using Unity.Entities;
using UnityEngine;

public class TargetA : MonoBehaviour
{
    public GameObject targetGO;
    public class Baker : Baker<TargetA>
    {
        public override void Bake(TargetA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Target()
            {
                targetEntity = GetEntity(authoring.targetGO, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct Target : IComponentData
{
    public Entity targetEntity;
}