using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;

namespace GDB.Character
{
    public partial class GravitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            
            foreach (var (player, phys, mass, entity) in SystemAPI
                         .Query<RefRW<PlayerControl>, RefRW<PhysicsVelocity>, RefRO<PhysicsMass>>().WithEntityAccess())
            {
                var idx = physicsWorld.GetRigidBodyIndex(entity);
                physicsWorld.ApplyLinearImpulse(idx, math.up() * -9.81f / mass.ValueRO.InverseMass + phys.ValueRW.Linear);
                
                //phys.ValueRW.Linear += new float3(0, -9.81f / mass.ValueRO.InverseMass, 0);
            }
        }
    }
}