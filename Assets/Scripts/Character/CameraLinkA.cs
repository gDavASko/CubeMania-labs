using Unity.Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GDB.Character
{
    public class CameraLinkA : MonoBehaviour
    {
        public class Baker : Baker<CameraLinkA>
        {
            public override void Bake(CameraLinkA authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                var Link = new CameraLink();
                
                AddComponentObject(e, Link);
            }
        }
    }

    public class CameraLink : IComponentData
    {
        public CinemachineCamera CameraFP;
        public CinemachineCamera CameraTP;
    }
}