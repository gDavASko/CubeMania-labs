using Unity.Entities;
using UnityEngine;

public class HealthA : MonoBehaviour
{
    public int health;
    public int maxHealth;

    public class Baker : Baker<HealthA>
    {
        public override void Bake(HealthA authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health()
            {
                health = authoring.health,
                maxHealth = authoring.maxHealth,
                onHPChanged = true,
            });
        }
    }
}

public struct Health: IComponentData
{
    public int health;
    public int maxHealth;

    public bool onHPChanged;
}