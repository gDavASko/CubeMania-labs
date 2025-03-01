using Unity.Entities;
using Unity.Transforms;

namespace GDB.Character
{
    partial struct PlayerRotSyncSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.ManagedAPI.GetSingleton<PlayerLink>();

            foreach (var playerTrsPos in SystemAPI.Query<RefRO<LocalToWorld>>().WithPresent<PlayerControl>())
            {
                player.Link.transform.rotation = playerTrsPos.ValueRO.Rotation;
            }
        }
    }
}