using Unity.Cinemachine;
using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    public class CameraObjectsA : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _fpCamera;
        [SerializeField] private CinemachineCamera _tpCamera;
        
        public class CameraObjectsBaker : Baker<CameraObjectsA>
        {
            public override void Bake(CameraObjectsA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var camEntity = new CameraObjects()
                {
                    CamRoot = authoring,
                };
                
                AddComponentObject(entity, camEntity);
            }
        }
    }

    public class CameraObjects : IComponentData
    {
        public CameraObjectsA CamRoot;
    }
}