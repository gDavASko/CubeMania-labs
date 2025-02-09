using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesA : MonoBehaviour
{
    public class Baker : Baker<EntitiesReferencesA>
    {
        public override void Bake(EntitiesReferencesA authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new EntitiesReferences { });
        }
    }
}

public struct EntitiesReferences : IComponentData
{ }
    