using Unity.Entities;
using UnityEngine;

public class MeleeAttackA : MonoBehaviour
{
    public float timerMax;
    public float colliderSize;
    public int damage;

    public class Baker : Baker<MeleeAttackA>
    {
        public override void Bake(MeleeAttackA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new MeleeAttack()
            {
                colliderSize = authoring.colliderSize,
                damage = authoring.damage,
                timerMax = authoring.timerMax,
                timer = authoring.timerMax,
            });
        }
    }
}

public struct MeleeAttack : IComponentData
{
    public float timer;
    public float timerMax;

    public float colliderSize;
    public int damage;
}
