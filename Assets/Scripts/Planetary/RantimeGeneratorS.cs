using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GDB.Planetary
{
    public struct GenParams
    {
        public CubeType MainBlockType;
        public CubeType DestroyableBlockType;
        public CubeType NonDestroyableBlockType;
        public CubeType ResourcesBlockType;

        public uint Centers;
        public uint MaxRadius;
        public uint MinNeighbors;
        public uint MaxNeighbors;

        public uint ResCount;
        public uint MinResNeighbors;
        public uint MaxResNeighbors;
        public uint MaxResBlocks;
    }
    
    public enum GenStep
    {
        GenUnbreakPoints = -3,
        GenUnbreakSecond = -2,
        GenUnbreakFinish = -1,
        GenSandPoints = 0,
        GenResourcesPoints = 1,
        GenCubeEntities = 2,
        GenFinish = 1000,
    }

    public partial class RantimeGeneratorS : SystemBase
    {
        private GenParams genParams = new GenParams()
        {
            MainBlockType = CubeType.Ice,
            DestroyableBlockType = CubeType.Sand,
            NonDestroyableBlockType = CubeType.Rock,
            ResourcesBlockType = CubeType.Resource, 
            Centers = 5,
            MaxRadius = 20,
            MinNeighbors = 1,
            MaxNeighbors = 1,
            ResCount = 3,
            MinResNeighbors = 2,
            MaxResNeighbors = 5,
            MaxResBlocks = 10,
        };
        
        // Создаем NativeList для результата (плоский список узлов)
        private Dictionary<float3, CubeType> nodes = new();
        private Queue<(uint, float3)> queueNodes = new();

        private GenStep curStep;
        private float3 center = new float3(0, 0, 0);

        private bool genInProcess = false;

        private int curBox = 0;
        private bool cubesInited = false;
        private int resCount = 0;
        private int curCenter = 0;
        private int curResCount = 0;

        private bool initedResources;

        private EntityCommandBuffer ecb;

        private Dictionary<CubeType, List<Entity>> cubeEntities = new();

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
            genInProcess = true;
            base.OnCreate();
            curStep = GenStep.GenSandPoints;
            initedResources = false;
            cubesInited = false;
            curResCount = 0;
            queueNodes.Enqueue((0, center));
        }

        public void RegenerateCubes(GenParams parameters)
        {
            if (genInProcess)
                return;
            
            genParams = parameters;

            ecb = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();
            
            foreach (var (cube, entity) in SystemAPI.Query<Cube>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
            
            nodes.Clear();
            queueNodes.Clear();
            OnCreate();
        }

        protected override void OnUpdate()
        {
            ecb = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();

            if (curStep == GenStep.GenFinish)
                return;

            if (!cubesInited)
            {
                foreach (var prefabBuffer in SystemAPI.Query<DynamicBuffer<PrefabBufferElement>>())
                {
                    if (prefabBuffer.IsEmpty)
                        return;

                    foreach (var c in prefabBuffer)
                    {
                        if (!cubeEntities.ContainsKey(c.cubeType))
                            cubeEntities[c.cubeType] = new List<Entity>();

                        cubeEntities[c.cubeType].Add(c.Prefab);
                    }
                }

                cubesInited = true;

                // Создаем центральный блок вручную
                CreateCube(center, CubeType.Ice, ecb);
                TryProcessTreeNode(2, 6, 6);
                cubesInited = true;
            }

            switch (curStep)
            {
                case GenStep.GenSandPoints:
                    if (!TryProcessTreeNode(genParams.MaxRadius, 
                            genParams.MinNeighbors, 
                            genParams.MaxNeighbors))
                    {
                        curCenter++;
                        if (curCenter < genParams.Centers)
                        {
                            int element = UnityEngine.Random.Range((int)(nodes.Count * 0.5f), nodes.Count);
                            queueNodes.Enqueue((0, nodes.Keys.ElementAt(element)));
                            queueNodes.Enqueue((0, nodes.Keys.ElementAt(element - 1)));
                            queueNodes.Enqueue((0, nodes.Keys.ElementAt(element - 2)));

                        }
                        else
                        {
                            curStep = GenStep.GenResourcesPoints;
                            resCount = 0;
                            queueNodes.Clear();
                            Debug.LogError("Completed sand points!");
                        }
                    }

                    break;

                case GenStep.GenResourcesPoints:
                    if (!TryAddResourceBlocks(genParams.MaxResBlocks, 
                            genParams.MinResNeighbors, 
                            genParams.MaxResNeighbors))
                    {
                        curResCount++;
                        if (curResCount >= genParams.ResCount)
                        {
                            curStep = GenStep.GenUnbreakPoints;
                            curBox = 0;
                            Debug.LogError("Completed resource points!");
                        }
                        else
                        {
                            resCount = 0;
                            queueNodes.Clear();
                            initedResources = false;
                            curStep = GenStep.GenResourcesPoints;
                        }
                    }

                    break;

                case GenStep.GenUnbreakPoints:
                    CubeType type = CubeType.Rock;

                    if (!TryGenerateUnbreakableTerrain(type))
                    {
                        Debug.LogError("Completed unbreaks points!");
                        curStep = GenStep.GenUnbreakSecond;
                    }

                    break;
                
                case GenStep.GenUnbreakSecond:

                    CubeType secondType = CubeType.Rock;

                    if (!TryGenerateUnbreakableSecondTerrain(secondType))
                    {
                        Debug.LogError("Completed unbreaks 2 points!");
                        curStep = GenStep.GenCubeEntities;
                    }

                    break;
                
                case GenStep.GenCubeEntities:

                    var resPoint = nodes.Keys.ElementAt(curBox);

                    curBox++;
                    CreateCube(resPoint, nodes[resPoint], ecb);

                    if (curBox >= nodes.Count)
                    {
                        curStep = GenStep.GenFinish;
                        Debug.LogError("Completed cubes generations!");
                        genInProcess = false;
                    }

                    break;
            }
        }

        public bool TryAddResourceBlocks(uint maxBlocks, uint minNeighbors, uint maxNeighbors)
        {
            if (nodes.Count == 0 || maxBlocks == 0)
                return false;

            Random random = new Random((uint)System.DateTime.UtcNow.Ticks);

            if (!initedResources)
            {
                int startIndex = random.NextInt(0, nodes.Count);
                float3 startPos = nodes.Keys.ElementAt(startIndex);

                queueNodes.Enqueue((0, startPos));
                nodes[startPos] = CubeType.Resource;
                initedResources = true;
            }

            if (queueNodes.Count > 0)
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
                    if (resCount >= maxBlocks)
                        continue;

                    resCount++;

                    float3 newPoint = curPos + directions[(int)neighbor];
                    queueNodes.Enqueue((curDepth + 1, newPoint));
                    nodes[newPoint] = CubeType.Resource;
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

            Random random = new Random((uint)System.DateTime.UtcNow.Millisecond + curRadius);
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

            foreach (var neighbor in neighbors) // Добавляем узел в плоский список
            {
                float3 point = curPos + directions[(int)neighbor];
                queueNodes.Enqueue((curRadius + 1, point));
                nodes[point] = CubeType.Sand;
                //Debug.LogError($"Added node: {point} for dist {curRadius + 1}");
            }

            return true;
        }

        private Entity CreateCube(float3 pos, CubeType type, EntityCommandBuffer cBuffer)
        {
            Entity spawnedEntity =
                cBuffer.Instantiate(cubeEntities[type][UnityEngine.Random.Range(0, cubeEntities[type].Count)]);

            cBuffer.SetComponent(spawnedEntity, new LocalTransform
            {
                Position = pos,
                Rotation = quaternion.identity,
                Scale = 1.05f
            });

            //Debug.LogError($"Created cube: index {spawnedEntity.Index} for point {pos}");
            return spawnedEntity;
        }

        public bool TryGenerateUnbreakableTerrain(CubeType unbreakableType)
        {
            // Получаем список ключей (координат) из словаря
            var keys = nodes.Keys.ToList();

            // Если текущий индекс вышел за пределы, сбрасываем его и увеличиваем глубину
            if (curBox >= nodes.Count)
            {
                curBox = 0;
                return false;
            }

            // Берем текущую точку
            float3 point = keys[curBox];

            // Проверяем, что элемент не является неразрушимым
            if (nodes[point] != unbreakableType)
            {
                // Проверяем, находится ли элемент на границе
                foreach (var direction in directions)
                {
                    float3 neighbor = point + direction;

                    // Если сосед отсутствует или не является неразрушимым, то это граница
                    if (!nodes.ContainsKey(neighbor) /*|| nodes[neighbor] != unbreakableType*/)
                    {
                        nodes[neighbor] = unbreakableType;
                        queueNodes.Enqueue((0, neighbor));
                    }
                }
            }

            // Переходим к следующему элементу
            curBox++;
            return true;
        }
        
        public bool TryGenerateUnbreakableSecondTerrain(CubeType unbreakableType)
        {
            if (queueNodes.Count == 0)
            {
                return false;
            }

            // Берем текущую точку
            var (dep, point) = queueNodes.Dequeue();

            // Проверяем, находится ли элемент на границе
            foreach (var direction in directions)
            {
                float3 neighbor = point + direction;

                // Если сосед отсутствует или не является неразрушимым, то это граница
                if (!nodes.ContainsKey(neighbor))
                {
                    nodes[neighbor] = unbreakableType;
                }
            }

            return true;
        }
    }
}