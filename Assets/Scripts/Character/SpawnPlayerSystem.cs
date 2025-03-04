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
            var camlinkFP = GameObject.Instantiate(spawner.VirtualCameraFP);
            var camlinkTP = GameObject.Instantiate(spawner.VirtualCameraTP);
            var player = GameObject.Instantiate(spawner.PlayerPrefab);

            var playerLink = SystemAPI.ManagedAPI.GetSingleton<PlayerLink>();
            playerLink.Link = player;
            
            var camLinkE = SystemAPI.ManagedAPI.GetSingleton<CameraLink>();
            camLinkE.CameraFP = camlinkFP.GetComponentInChildren<CinemachineCamera>();
            camLinkE.CameraTP = camlinkTP.GetComponentInChildren<CinemachineCamera>();
            
            

            var target = camLinkE.CameraFP.Target;
            target.TrackingTarget = player.transform;
            camLinkE.CameraFP.Target = target;
            
            target = camLinkE.CameraTP.Target;
            target.TrackingTarget = player.transform;
            camLinkE.CameraTP.Target = target;

            camLinkE.CameraFP.Priority = 10;
            camLinkE.CameraTP.Priority = 0;
            
            spawned = true;
        }
    }
}