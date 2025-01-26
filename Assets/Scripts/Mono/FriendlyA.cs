using Unity.Entities;
using UnityEngine;

public class FriendlyA : MonoBehaviour
{
    public class Baker : Baker<FriendlyA>
    {
        public override void Bake(FriendlyA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Friendly());
        }
    }
}

public struct Friendly : IComponentData
{
    
}
