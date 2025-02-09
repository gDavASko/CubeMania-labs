using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace GDB.Planetary
{
    partial struct GravitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physic = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var colls = physic.CollisionWorld;

            foreach (var (gravity, trs, entity) in
                SystemAPI.Query<RefRO<Gravity>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                if(gravity.ValueRO.gravityBase == Entity.Null)
                {
                    continue;
                }

                var baseLt = SystemAPI.GetComponentRO<LocalTransform>(gravity.ValueRO.gravityBase);

                var gravityDir = baseLt.ValueRO.Position - trs.ValueRO.Position;
                gravityDir = math.normalize(gravityDir);


            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}