using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    public class PlayerSpawnerA : MonoBehaviour
    {
        [SerializeField] public GameObject PlayerPrefab;
        [SerializeField] public GameObject CameraMain;
        [SerializeField] public GameObject VirtualCameraFP;
        [SerializeField] public GameObject VirtualCameraTP;

        public class Baker : Baker<PlayerSpawnerA>
        {
            public override void Bake(PlayerSpawnerA authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                
                var Link = new PlayerPrefabs()
                {
                    PlayerPrefab = authoring.PlayerPrefab,
                    CameraMain = authoring.CameraMain,
                    VirtualCameraFP = authoring.VirtualCameraFP,
                    VirtualCameraTP = authoring.VirtualCameraTP
                };
                AddComponentObject(e, Link);
            }
        }
    }
    
    public class PlayerPrefabs : IComponentData
    {
        public GameObject PlayerPrefab;
        public GameObject CameraMain;
        public GameObject VirtualCameraFP;
        public GameObject VirtualCameraTP;
    }
}