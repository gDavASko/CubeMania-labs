using Unity.Entities;
using UnityEngine;

public class BulletA : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;

    public class Baker : Baker<BulletA>
    {
        public override void Bake(BulletA authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet()
            {
                speed = authoring.speed,
                damage = authoring.damage
            });
        }
    }
}

public struct Bullet : IComponentData
{
    public float speed;
    public int damage;
}