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
                Current = authoring.health,
                Max = authoring.maxHealth,
                onHPChanged = true,
            });
        }
    }
}

public struct Health: IComponentData
{
    public int Current;
    public int Max;

    public bool onHPChanged;
}