using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerA : MonoBehaviour
{
    public float timerMax;

    public class Baker : Baker<ZombieSpawnerA>
    {
        public override void Bake(ZombieSpawnerA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new ZombieSpawner()
            {                
                timer = 0,
                timerMax = authoring.timerMax
            });
        }
    }
}

public struct ZombieSpawner: IComponentData
{
    public float timer;
    public float timerMax;
}