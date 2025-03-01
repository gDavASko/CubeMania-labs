using Unity.Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GDB.Character
{
    public class CameraLinkA : MonoBehaviour
    {
        [SerializeField] public Vector3 Offset;

        public class Baker : Baker<CameraLinkA>
        {
            public override void Bake(CameraLinkA authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                var Link = new CameraLink()
                {
                    Offfset = authoring.Offset
                };
                
                AddComponentObject(e, Link);
            }
        }
    }

    public class CameraLink : IComponentData
    {
        public CinemachineCamera Camera;
        public float3 Offfset;
    }
}