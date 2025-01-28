using Unity.Entities;
using UnityEngine;

public class ShootAttackA : MonoBehaviour
{
    public int damage = 5;
    public float timerMax = 0.2f;

    public class Baker : Baker<ShootAttackA>
    {
        public override void Bake(ShootAttackA authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootAttack()
            {
                damage = authoring.damage,
                timerMax = authoring.timerMax
            });
        }
    }
}

public struct ShootAttack: IComponentData
{
    public int damage;
    public float timer;
    public float timerMax;
}