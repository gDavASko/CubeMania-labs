using Unity.Entities;
using Unity.Transforms;

namespace GDB.Character
{
    partial struct PlayerPosSyncSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.ManagedAPI.GetSingleton<PlayerLink>();

            foreach (var playerTrsPos in SystemAPI.Query<RefRO<LocalToWorld>>().WithPresent<PlayerControl>())
            {
                player.Link.transform.position = playerTrsPos.ValueRO.Position;
            }
        }
    }
}