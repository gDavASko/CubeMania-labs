using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesA : MonoBehaviour
{
    public GameObject bulletGO;

    public class Baker : Baker<EntitiesReferencesA>
    {
        public override void Bake(EntitiesReferencesA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new EntitiesReferences()
            {
                bulletEntity = GetEntity(authoring.bulletGO, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct EntitiesReferences: IComponentData
{
    public Entity bulletEntity;
}