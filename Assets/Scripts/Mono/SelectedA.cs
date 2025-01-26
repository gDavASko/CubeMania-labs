using Unity.Entities;
using UnityEngine;

public class SelectedA : MonoBehaviour
{
    public GameObject visualGO;
    public float showScale = 2f;

    public class Baker : Baker<SelectedA>
    {
        public override void Bake(SelectedA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Selected()
            {
                visualEntity = GetEntity(authoring.visualGO, TransformUsageFlags.Dynamic),
                showScale = authoring.showScale,
            });
            SetComponentEnabled<Selected>(e, false);
        }
    }
}

public struct Selected: IComponentData, IEnableableComponent
{
    public Entity visualEntity;
    public float showScale;

    public bool onSelected;
    public bool onDeselected;
}

