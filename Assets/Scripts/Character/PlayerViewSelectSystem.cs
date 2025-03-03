using GDB.Character;
using GDB.Planetary;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerViewSelectSystem : ISystem
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
        var selection = SystemAPI.GetSingletonEntity<SelectionCube>();
        
        if (_playerQuery.IsEmpty || _damagableQuery.IsEmpty) 
            return;
        
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (inputData, transform, entity) 
                 in SystemAPI.Query<RefRO<PlayerInputData>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            float3 origin = transform.ValueRO.Position;
            float3 direction = transform.ValueRO.Forward; 
            float maxDistance = 6f;
            
            var raycastInput = new RaycastInput
            {
                Start = origin,
                End = origin + direction * maxDistance,
                Filter = _colFilter
            };

            if (physicsWorld.CastRay(raycastInput, out var hit))
            {
                var hitEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

                if (hitEntity == Entity.Null && !SystemAPI.HasComponent<LocalTransform>(hitEntity))
                    continue;
                
                var hitTrs = SystemAPI.GetComponent<LocalTransform>(hitEntity); 

                if(math.distancesq(hitTrs.Position, origin) <= maxDistance * maxDistance)
                {
                    SystemAPI.SetComponent(selection, LocalTransform.FromPosition(hitTrs.Position));
                    //Debug.LogError($"Selected object {hitEntity.Index}");
                }
                else
                {
                    SystemAPI.SetComponent(selection, LocalTransform.FromPosition(new float3(1,1,1) * float.MaxValue));
                }
            }
            else
            {
                SystemAPI.SetComponent(selection, LocalTransform.FromPosition(new float3(1,1,1) * float.MaxValue));
            }
        }
    }
}
