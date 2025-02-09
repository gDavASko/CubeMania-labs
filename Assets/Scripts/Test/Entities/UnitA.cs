using Unity.Entities;
using UnityEngine;

public class UnitA : MonoBehaviour
{
    public Fraction Fraction;

    public class Baker : Baker<UnitA>
    {
        public override void Bake(UnitA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Unit()
            {
                fraction = authoring.Fraction
            });
        }
    }
}

public struct Unit: IComponentData
{
    public Fraction fraction;
}
