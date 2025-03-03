using Unity.Entities;
using UnityEngine;

namespace GDB.Common.Damagables
{
    public class DamagableA : MonoBehaviour
    {
        [SerializeField] private DamageType[] _damageType;
        [SerializeField] private int _maxHealth = 100;

        public class Baker : Baker<DamagableA>
        {
            public override void Bake(DamagableA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Health()
                {
                    Current = authoring._maxHealth,
                    Max = authoring._maxHealth
                });
                
                foreach (var damageType in authoring._damageType)
                {
                    switch (damageType)
                    {
                        case DamageType.Common:
                            AddComponent(entity, new DamagableCommon());
                            break;
                        case DamageType.Explosive:
                            AddComponent(entity, new DamagableExplosive());
                            break;
                        case DamageType.Fire:
                            AddComponent(entity, new DamagableFire());
                            break;
                        case DamageType.Ice:
                            AddComponent(entity, new DamagableIce());
                            break;
                        case DamageType.Electric:
                            AddComponent(entity, new DamagableElectric());
                            break;
                        case DamageType.Radiation:
                            AddComponent(entity, new DamagableRadiation());
                            break;
                        case DamageType.Plasma:
                            AddComponent(entity, new DamagablePlasma());
                            break;
                        case DamageType.Laser:
                            AddComponent(entity, new DamagableLaser());
                            break;
                        case DamageType.Chemical:
                            AddComponent(entity, new DamagableChemical());
                            break;
                        case DamageType.Biological:
                            AddComponent(entity, new DamagableBiological());
                            break;
                    }
                }
            }
        }
    }
    
    public struct DamagableCommon : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagableFire : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagableIce : IComponentData, IEnableableComponent
    {
    }
    
    
    public struct DamagableExplosive : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagableElectric : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagableRadiation : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagablePlasma : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagableLaser : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagableChemical : IComponentData, IEnableableComponent
    {
    }
    
    public struct DamagableBiological : IComponentData, IEnableableComponent
    {
    }
}