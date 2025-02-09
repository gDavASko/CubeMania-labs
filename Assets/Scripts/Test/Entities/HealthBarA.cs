using Unity.Entities;
using UnityEngine;

public class HealthBarA : MonoBehaviour
{
    public GameObject barVisualGO;
    public GameObject healthGO;

    public class Baker : Baker<HealthBarA>
    {
        public override void Bake(HealthBarA authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HealthBar()
            {
                barVisualEntity = GetEntity(authoring.barVisualGO, TransformUsageFlags.NonUniformScale),
                healthEntity = GetEntity(authoring.healthGO, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct HealthBar : IComponentData
{
    public Entity barVisualEntity;
    public Entity healthEntity;
}