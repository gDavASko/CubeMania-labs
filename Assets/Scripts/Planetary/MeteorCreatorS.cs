using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GDB.Planetary
{
    public struct MeteorCreatedTag : IComponentData { }


    [BurstCompile]
    public partial struct MeteorCreatorS : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Проверяем, существует ли уже метка MeteorCreatedTag
            if (SystemAPI.HasSingleton<MeteorCreatedTag>())
                return; // Если метка есть, выходим из системы

            // Получаем список префабов
            foreach (var prefabBuffer in SystemAPI.Query<DynamicBuffer<PrefabBufferElement>>())
            {
                if (prefabBuffer.IsEmpty)
                    continue;

                // Получаем EntityCommandBuffer
                var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.World.Unmanaged);

                var _random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);

                // Выбираем случайный префаб
                int randomIndex = _random.NextInt(0, prefabBuffer.Length);
                Entity randomPrefab = prefabBuffer[randomIndex].Prefab;

                // Создаем сущность на основе выбранного префаба
                Entity spawnedEntity = ecb.Instantiate(randomPrefab);

                // Устанавливаем позицию для новой сущности через ECB
                ecb.SetComponent(spawnedEntity, new LocalTransform
                {
                    Position = new float3(0, 0, 0), // Позиция по умолчанию
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                // Создаем сущность-маркер через ECB
                Entity markerEntity = ecb.CreateEntity();
                ecb.AddComponent<MeteorCreatedTag>(markerEntity);

                // Выходим из системы после спавна одного префаба
                break;
            }
        }

        /*public void OnUpdate(ref SystemState state)
        {
            Debug.LogError("MeteorCreatorS OnUpdate called");

            if (!SystemAPI.TryGetSingleton(out CubeReferences entitiesReferences))
            {
                Debug.LogError("CubeReferences not found!");
                return;
            }        

            // Проверяем, создавались ли уже кубы (если да, то выходим)
            if (!SystemAPI.QueryBuilder().WithAll<MeteorCreatedTag>().Build().IsEmptyIgnoreFilter)
                return;

            // Получаем командный буфер

            Debug.LogError("Found CubeReferences!");
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.World.Unmanaged);          

            ref var cubes = ref entitiesReferences.CubeEntities.Value.CubeEntities;
            float xOffset = 0f;

            Debug.LogError("Checking for CubeReferences...");
            for (int i = 0; i < cubes.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Debug.LogError($"Instantiating cubeEntity {i}: " +
                        $"{cubes[i].cubeEntity.Index}, Version: {cubes[i].cubeEntity.Version}");

                    Entity cubeEntity = ecb.Instantiate(cubes[i].cubeEntity);
                    ecb.SetComponent(cubeEntity, new LocalTransform
                    {
                        Position = new float3(xOffset, j, 0),
                        Scale = 1f
                    });
                }

                xOffset += 2f;
            }

            // Создаем сущность-маркер
            Entity markerEntity = ecb.CreateEntity();
            ecb.AddComponent<MeteorCreatedTag>(markerEntity);
        }*/
    }
}