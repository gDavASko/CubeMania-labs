using Unity.Entities;
using UnityEngine;

public class ShootLightA : MonoBehaviour
{
    public float timerMax;

    public class Baker : Baker<ShootLightA>
    {
        public override void Bake(ShootLightA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new ShootLight()
            {
                timer = authoring.timerMax,
                timerMax = authoring.timerMax,
            });
        }
    }
}

public struct ShootLight: IComponentData
{
    public float timer;
    public float timerMax;
}