using Unity.Cinemachine;
using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class SpawnPlayerSystem : SystemBase
    {
        private bool spawned = false;
        protected override void OnUpdate()
        {
            if(spawned)
                return;
            
            var spawner = SystemAPI.ManagedAPI.GetSingleton<PlayerPrefabs>();

            GameObject.Instantiate(spawner.CameraMain);
            var camlink = GameObject.Instantiate(spawner.VirtualCamera);
            var player = GameObject.Instantiate(spawner.PlayerPrefab);

            var playerLink = SystemAPI.ManagedAPI.GetSingleton<PlayerLink>();
            playerLink.Link = player;
            
            var camLinkE = SystemAPI.ManagedAPI.GetSingleton<CameraLink>();
            camLinkE.Camera = camlink.GetComponentInChildren<CinemachineCamera>();

            var target = camLinkE.Camera.Target;
            target.TrackingTarget = player.transform;

            camLinkE.Camera.Target = target;
            
            spawned = true;
        }
    }
}