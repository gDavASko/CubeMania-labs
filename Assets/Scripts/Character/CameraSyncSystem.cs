using Unity.Entities;
using Unity.Transforms;

namespace GDB.Character
{
    partial struct CameraSyncSystem : ISystem
    {

        public void OnUpdate(ref SystemState state)
        {
            var camLink = SystemAPI.ManagedAPI.GetSingleton<CameraLink>();
            
            foreach(var lt in SystemAPI.Query<RefRO<LocalToWorld>>().WithPresent<PlayerControl>())
            {
                camLink.Camera.transform.position = lt.ValueRO.Position + camLink.Offfset;
                break;
            }
        }
    }
}