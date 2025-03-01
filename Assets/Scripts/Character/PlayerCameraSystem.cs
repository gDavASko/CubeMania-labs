using Unity.Entities;
using Unity.Transforms;

namespace GDB.Character
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]

    partial struct PlayerCameraSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            CameraObjects camObj = null;
            Entity camEntity = default;
            foreach (var (cam, e )
                     in SystemAPI.Query<CameraObjects>().WithEntityAccess())
            {
                camObj = cam;
                camEntity = e;
                break;
            }

            if (camObj == null)
                return;

            foreach (var (player, trs, localTrs)
                     in SystemAPI.Query<RefRO<PlayerControl>, RefRO<LocalToWorld>, RefRO<LocalTransform>>())
            {
                camObj.CamRoot.transform.position = trs.ValueRO.Position;
                SystemAPI.SetComponent(camEntity, LocalTransform.FromPosition(localTrs.ValueRO.Position));
                return;
            }
        }
    }
}