using GDB.Character;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerShootSystem : ISystem
{
    private EntityQuery _playerQuery;
    private EntityQuery _damagableQuery;
    CollisionFilter _colFilter;

    public void OnCreate(ref SystemState state)
    {
        _playerQuery = state.GetEntityQuery(ComponentType.ReadOnly<PlayerInputData>(), ComponentType.ReadOnly<LocalToWorld>());
        _damagableQuery = state.GetEntityQuery(ComponentType.ReadWrite<Health>());
        
        _colFilter = new CollisionFilter()
        {
            GroupIndex = 0,
            BelongsTo = ~0u,
            CollidesWith = 1u << GameAssets.CUBES_LAYER,
        };
    }

    public void OnUpdate(ref SystemState state)
    {
        if (_playerQuery.IsEmpty || _damagableQuery.IsEmpty) return;

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (inputData, transform, entity) 
                 in SystemAPI.Query<RefRO<PlayerInputData>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            if (!inputData.ValueRO.ShootButton)
                continue;

            float3 origin = transform.ValueRO.Position;
            float3 direction = transform.ValueRO.Forward; // Стрелять вперёд из центра экрана
            float maxDistance = 6f;
            
            Debug.DrawRay(origin, direction * maxDistance, Color.red, 1f, false);
            
            var raycastInput = new RaycastInput
            {
                Start = origin,
                End = origin + direction * maxDistance,
                Filter = _colFilter
            };

            if (physicsWorld.CastRay(raycastInput, out var hit))
            {
                var hitEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

                if (SystemAPI.HasComponent<Health>(hitEntity))
                {
                    var health = SystemAPI.GetComponentRW<Health>(hitEntity);
                    health.ValueRW.Current -= 25;
                    health.ValueRW.onHPChanged = true;

                    if (health.ValueRW.Current <= 0)
                    {
                        ecb.DestroyEntity(hitEntity);
                    }
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
