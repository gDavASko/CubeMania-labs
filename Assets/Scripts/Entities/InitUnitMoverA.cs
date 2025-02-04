using Unity.Entities;
using UnityEngine;

public class InitUnitMoverA : MonoBehaviour
{
    
    public class Baker : Baker<InitUnitMoverA>
    {
        public override void Bake(InitUnitMoverA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new InitUnitMover());
        }
    }
}

public struct InitUnitMover : IComponentData 
{ 
}