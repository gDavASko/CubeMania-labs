using Unity.Entities;
using UnityEngine;

public class ZombieA : MonoBehaviour
{
    public class Baker : Baker<ZombieA> 
    {
        public override void Bake(ZombieA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Zombie());
        }
    }
}

public struct Zombie: IComponentData
{

}