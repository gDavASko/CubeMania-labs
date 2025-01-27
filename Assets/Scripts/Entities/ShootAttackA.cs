using Unity.Entities;
using UnityEngine;

public class ShootAttackA : MonoBehaviour
{
    public float timerMax = 0.2f;

    public class Baker : Baker<ShootAttackA>
    {
        public override void Bake(ShootAttackA authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootAttack()
            {
                timerMax = authoring.timerMax
            });
        }
    }
}

public struct ShootAttack: IComponentData
{
    public float timer;
    public float timerMax;
}