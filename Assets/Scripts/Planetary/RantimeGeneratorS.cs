using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GDB.Planetary
{
    public enum GenStep
    {
        GenSandPoints = 0,
        GenSandCubes = 1,
        GenResourcesPoints = 2,
        GenResourceCube = 3,
        GetFinish = 1000,
    }
    
    public partial class RantimeGeneratorS : SystemBase
    {
        // Создаем NativeList для результата (плоский список узлов)
        private Dictionary<float3, Entity> sandNodes = new();
        private Dictionary<float3, Entity> resourceNodes = new();
        
        private Queue<(uint, float3)> queueNodes = new();

        private GenStep curStep;

        private float3 center = new float3(0, 0, 0);

        private int curBox = 0;
        private int curResourceBox = 0;

        private int maxResources = 10;
        private int curResCount = 0;

        private bool completedPoints;
        private bool completedResourcesPoints;
        private bool completedCubes;

        private bool initedResources;

        private EntityCommandBuffer ecb;

        private Entity baseCube = Entity.Null;
        private Entity sandCube = Entity.Null;
        private Entity ResourceCube = Entity.Null;
        private Entity rockCube = Entity.Null;

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
            curStep = GenStep.GenSandPoints;    
            completedPoints = false;
            completedResourcesPoints = false;
            completedCubes = false;
            initedResources = false;

            curResCount = 0;
            queueNodes.Enqueue((0, center));
        }

        protected override void OnUpdate()
        {
            ecb = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();

            if (curStep == GenStep.GetFinish)
                return;

            if (baseCube == Entity.Null)
            {
                foreach (var prefabBuffer in SystemAPI.Query<DynamicBuffer<PrefabBufferElement>>())
                {
                    if (prefabBuffer.IsEmpty)
                        return;

                    foreach (var c in prefabBuffer)
                    {
                        if (c.cubeType == CubeType.Ice)
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
                        else if (c.cubeType == CubeType.Resource)
                        {
                            ResourceCube = c.Prefab;
                        }

                        if (baseCube != Entity.Null && sandCube != Entity.Null && rockCube != Entity.Null &&
                            ResourceCube != Entity.Null)
                        {
                            break;
                        }
                    }
                }

                // Создаем центральный блок вручную
                CreateCube(center, baseCube, ecb);
                TryProcessTreeNode(2, 6, 6);
            }

            if (baseCube == Entity.Null)
                return;

            switch (curStep)
            {
                case GenStep.GenSandPoints:
                    uint maxRadius = 100;
                    uint maxNeighbors = 5;
                    uint minNeighbors = 1;

                    if (!TryProcessTreeNode(maxRadius, minNeighbors, maxNeighbors))
                    {
                        curStep = GenStep.GenSandCubes;
                        curBox = 0;
                        queueNodes.Clear();
                        Debug.LogError("Completed points!");
                    }

                    break;

                case GenStep.GenSandCubes:
                    var point = sandNodes.Keys.ElementAt(curBox);
                    sandNodes[point] = CreateCube(point, sandCube, ecb);
                    curBox++;

                    if (curBox >= sandNodes.Count)
                        curStep = GenStep.GenResourcesPoints;
                    break;

                case GenStep.GenResourcesPoints:
                    uint maxBlocks = 10;
                    uint maxResNeighbors = 4;
                    uint minResNeighbors = 1;

                    if (!TryAddResourceBlocks(maxBlocks, minResNeighbors, maxResNeighbors))
                    {
                        curBox = 0;
                        curStep = GenStep.GenResourceCube;
                    }

                    break;

                case GenStep.GenResourceCube:

                    var resPoint = resourceNodes.Keys.ElementAt(curBox);

                    curBox++;
                    if (resourceNodes[resPoint] != Entity.Null)
                        return;
                    
                    resourceNodes[resPoint] = CreateCube(resPoint, ResourceCube, ecb);

                    if (curBox >= resourceNodes.Count)
                    {
                        curResCount++;
                        if (curResCount >= maxResources)
                        {
                            curStep = GenStep.GetFinish;
                            Debug.LogError("Completed cubes!");
                        }
                        else
                        {
                            resourceNodes.Clear();
                            queueNodes.Clear();
                            initedResources = false;
                            curStep = GenStep.GenResourcesPoints;
                        }
                    }
                    break;
            }
        }

        public bool TryAddResourceBlocks(uint maxBlocks, uint minNeighbors, uint maxNeighbors)
        {
            if (sandNodes.Count == 0 || maxBlocks == 0)
                return false;

            Random random = new Random((uint)System.DateTime.UtcNow.Millisecond + 2);
            
            if (!initedResources)
            {
                int startIndex = random.NextInt(0, sandNodes.Count);
                float3 startPos = sandNodes.Keys.ElementAt(startIndex);

                if (resourceNodes.ContainsKey(startPos))
                    return true;
                
                queueNodes.Enqueue((0, startPos));
                resourceNodes[startPos] = Entity.Null;
                initedResources = true;
            }

            if(queueNodes.Count > 0)
            {
                var (curDepth, curPos) = queueNodes.Dequeue();
                
                if (curDepth >= maxBlocks)
                    return true;
        
                uint neighborsCount = random.NextUInt(minNeighbors, maxNeighbors);
                List<uint> neighbors = new();

                while (neighbors.Count < neighborsCount)
                {
                    uint newIndex = random.NextUInt(0, (uint)directions.Length);
                    if (neighbors.Contains(newIndex))
                        continue;
                    neighbors.Add(newIndex);
                }

                foreach (var neighbor in neighbors)
                {
                    if (resourceNodes.Count >= maxBlocks)
                        continue;
                    
                    float3 newPoint = curPos + directions[(int)neighbor];

                    if (resourceNodes.ContainsKey(newPoint))
                        continue;

                    if (sandNodes.ContainsKey(newPoint))
                    {
                        if (sandNodes[newPoint] != Entity.Null)
                        {
                            ecb.DestroyEntity(sandNodes[newPoint]);
                        }

                        sandNodes.Remove(newPoint);
                    }
                    
                    queueNodes.Enqueue((curDepth + 1, newPoint));
                    resourceNodes[newPoint] = Entity.Null;
                    
                    Debug.LogError($"Added resource node: {newPoint} for dist {curDepth + 1}");
                }

                return true;
            }

            return false;
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
                float3 point = curPos + directions[(int)neighbor];
                queueNodes.Enqueue((curRadius + 1, point));
                sandNodes[point] = Entity.Null;
                //Debug.LogError($"Added node: {point} for dist {curRadius + 1}");
            }

            return true;
        }

        private Entity CreateCube(float3 pos, Entity cube, EntityCommandBuffer cBuffer)
        {
            Entity spawnedEntity = cBuffer.Instantiate(cube);

            cBuffer.SetComponent(spawnedEntity, new LocalTransform
            {
                Position = pos,
                Rotation = quaternion.identity,
                Scale = 1f
            });
            
            //Debug.LogError($"Created cube: index {spawnedEntity.Index} for point {pos}");
            return spawnedEntity;
        }
    }
}