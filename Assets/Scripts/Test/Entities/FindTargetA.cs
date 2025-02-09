using Unity.Entities;
using UnityEngine;

public class FindTargetA : MonoBehaviour
{
    public float range = 10f;
    public Fraction targetFraction;
    public float timerMax = 1f;

    public class Baker : Baker<FindTargetA>
    {
        public override void Bake(FindTargetA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new FindTarget()
            {
                range = authoring.range,
                targetFraction = authoring.targetFraction,
                timerMax = authoring.timerMax
            });
        }
    }
}

public struct FindTarget: IComponentData
{
    public float range;
    public Fraction targetFraction;

    public float timer;
    public float timerMax;
}