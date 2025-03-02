using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace GDB.Common.Damagables
{
    public enum DamageType
    {
        Common = 0,
        Explosive = 1,
        Fire = 2,
        Ice = 3,
        Electric = 4,
        Radiation = 5,
        Plasma = 6,
        Laser = 7,
        Chemical = 8,
        Biological = 9,
    }

    public class DamagableA : MonoBehaviour
    {
        [SerializeField] private DamageType[] _damageType;

        public class Baker : Baker<DamagableA>
        {
            public override void Bake(DamagableA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var damagable = new Damagable
                {
                    damageTypes = new NativeArray<DamageType>(authoring._damageType, Allocator.Persistent)
                };
                AddComponent(entity, damagable);
            }
        }
    }

    public struct Damagable : IComponentData, IEnableableComponent
    {
        public NativeArray<DamageType> damageTypes;
    }
}