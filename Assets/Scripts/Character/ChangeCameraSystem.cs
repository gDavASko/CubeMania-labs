using Unity.Entities;

namespace GDB.Character
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class ChangeCameraSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var input in SystemAPI.Query<RefRW<PlayerInputData>>())
            {
                if (input.ValueRO.CamChangeNeedProcess)
                {
                    var camLinkE = SystemAPI.ManagedAPI.GetSingleton<CameraLink>();
                    var priorityFP = camLinkE.CameraFP.Priority;
                    camLinkE.CameraFP.Priority = camLinkE.CameraTP.Priority;
                    camLinkE.CameraTP.Priority = priorityFP;

                    input.ValueRW.CamChangeNeedProcess = false;
                    break;
                }
            }
        }
    }
}