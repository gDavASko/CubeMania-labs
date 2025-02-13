using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GDB.Planetary
{
    public partial class RantimeGeneratorS : SystemBase
    {
        // Создаем NativeList для результата (плоский список узлов)
        private HashSet<float3> resultNodes = new();
        private Queue<float3> positions;
        private uint lastIndex;
        private Queue<(uint, float3)> queueNodes = new();

        private float3 center = new float3(0, 0, 0);

        private bool completedPoints;
        private bool completedCubes;

        private EntityCommandBuffer ecb;

        Entity baseCube = Entity.Null;
        Entity sandCube = Entity.Null;
        Entity rockCube = Entity.Null;

        private readonly float3[] directions = new float3[6]
        {
            new float3(1, 0, 0),
            new float3(-1, 0, 0),
            new float3(0, 1, 0),
            new float3(0, -1, 0),
            new float3(0, 0, 1),
            new float3(0, 0, -1)
        };


        protected override void OnCreate()
        {
            base.OnCreate();
            lastIndex = 0;
            completedPoints = false;
            completedCubes = false;
            queueNodes.Enqueue((0, center));

            /*ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(EntityManager.World.Unmanaged);*/
        }

        protected override void OnUpdate()
        {
            ecb = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();
            
            if (completedPoints && completedCubes)
                return;

            if (baseCube == Entity.Null)
            {
                foreach (var prefabBuffer in SystemAPI.Query<DynamicBuffer<PrefabBufferElement>>())
                {
                    if (prefabBuffer.IsEmpty)
                        return;

                    foreach (var c in prefabBuffer)
                    {
                        if (c.cubeType == CubeType.Resource)
                        {
                            baseCube = c.Prefab;
                        }
                        else if (c.cubeType == CubeType.Sand)
                        {
                            sandCube = c.Prefab;
                        }
                        else if (c.cubeType == CubeType.Rock)
                        {
                            rockCube = c.Prefab;
                        }

                        if (baseCube != Entity.Null && sandCube != Entity.Null && rockCube != Entity.Null)
                        {
                            break;
                        }
                    }
                }
                
                // Создаем центральный блок вручную
                CreateCube(center, baseCube, ecb);
                TryProcessTreeNode(2, 4, 6);
            }

            if (baseCube == Entity.Null)
                return;

            if (!completedPoints)
            {
                uint maxRadius = 10;
                uint maxNeighbors = 4;
                uint minNeighbors = 2;

                if (!TryProcessTreeNode(maxRadius, minNeighbors, maxNeighbors))
                {
                    positions = new Queue<float3>(resultNodes);
                    completedPoints = true;
                    Debug.LogError("Completed points!");
                }
            }
            else if(positions.Count > 0)
            {
                CreateCube(positions.Dequeue(), sandCube, ecb);
            }
            else
            {
                Debug.LogError("Completed cubes!");
                completedCubes = true;
            }

            /*// Преобразуем плоский список в координаты
             NativeArray<float3> resultPoints = new NativeArray<float3>(resultNodes.Length, Allocator.TempJob);
             for (int i = 0; i < resultNodes.Length; i++)
             {
                 resultPoints[i] = resultNodes[i].position;
             }

             Debug.Break();
             

            /*var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb2 = ecbSystem.CreateCommandBuffer(state.World.Unmanaged);

            var spawnJob = new SpawnCubesJob
            {
                positions = resultPoints,
                ecb = ecb2.AsParallelWriter(),
                cubePrefab = sandCube
            };

            // Запускаем Job
            JobHandle jobHandle2 = spawnJob.Schedule(resultPoints.Length, 64); // Параллельное выполнение
            jobHandle2.Complete(); // Завершаем Job перед использованием ECB


            Debug.LogError("Completed2!");*/
        }
        
        public bool TryProcessTreeNode(uint maxRadius, uint minNeighbors, uint maxNeighbors)
        {
            if (queueNodes.Count == 0)
                return false;

            var (curRadius, curPos) = queueNodes.Dequeue();
            
            if (curRadius >= maxRadius)
                return true;
            
            Random random = new Random((uint)System.DateTime.UtcNow.Millisecond + 1);
            uint neighborsCount = random.NextUInt(minNeighbors, maxNeighbors);

            List<uint> neighbors = new();

            uint newIndex = 0;
            while (neighbors.Count < neighborsCount) // find random neighbors
            {
                newIndex = random.NextUInt(0, (uint)directions.Length);
                if (neighbors.Contains(newIndex))
                    continue;
                
                neighbors.Add(newIndex);
            }

            foreach(var neighbor in neighbors)// Добавляем узел в плоский список
            {
                lastIndex++;
                float3 point = curPos + directions[(int)neighbor];
                queueNodes.Enqueue((curRadius + 1, point));
                resultNodes.Add(point);
                //Debug.LogError($"Added node: {point} for dist {curRadius + 1}");
            }

            return true;
        }

        private void CreateCube(float3 pos, Entity cube, EntityCommandBuffer cBuffer)
        {
            Entity spawnedEntity = cBuffer.Instantiate(cube);

            cBuffer.SetComponent(spawnedEntity, new LocalTransform
            {
                Position = pos,
                Rotation = quaternion.identity,
                Scale = 1f
            });
            
            //Debug.LogError($"Created cube: index {spawnedEntity.Index} for point {pos}");
        }
    }
}