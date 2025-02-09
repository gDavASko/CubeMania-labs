using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerA : MonoBehaviour
{
    public float timerMax;
    public float minRndDist;
    public float maxRndDist;

    public class Baker : Baker<ZombieSpawnerA>
    {
        public override void Bake(ZombieSpawnerA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new ZombieSpawner()
            {                
                timer = 0,
                timerMax = authoring.timerMax,
                minRndDist = authoring.minRndDist,
                maxRndDist = authoring.maxRndDist,
            });
        }
    }
}

public struct ZombieSpawner: IComponentData
{
    public float timer;
    public float timerMax;
    public float minRndDist;
    public float maxRndDist;
}