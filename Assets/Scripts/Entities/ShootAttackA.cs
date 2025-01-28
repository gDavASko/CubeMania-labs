using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootAttackA : MonoBehaviour
{
    public float attackDist = 5f;
    public int damage = 5;
    public float timerMax = 0.2f;
    public Transform _bulletSpawnPos;

    public class Baker : Baker<ShootAttackA>
    {
        public override void Bake(ShootAttackA authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootAttack()
            {
                attackDist = authoring.attackDist,
                damage = authoring.damage,
                timerMax = authoring.timerMax,
                bulletSpawnPoint = authoring._bulletSpawnPos.localPosition,
            });
        }
    }
}

public struct ShootAttack: IComponentData
{
    public float attackDist;
    public int damage;
    public float timer;
    public float timerMax;
    public float3 bulletSpawnPoint;
}