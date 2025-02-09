using Unity.Entities;
using UnityEngine;

namespace GDB.Planetary
{
    public class GravityA : MonoBehaviour
    {
        [SerializeField] private float mass = 100f;

        public class Baker : Baker<GravityA>
        {
            public override void Bake(GravityA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Gravity()
                {
                    mass = authoring.mass,
                });
            }
        }
    }

    public struct Gravity : IComponentData
    {
        public float mass;
        public Entity gravityBase;
    }
}