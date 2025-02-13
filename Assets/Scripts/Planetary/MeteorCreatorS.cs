using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GDB.Planetary
{
   /*public struct MeteorCreatedTag : IComponentData { }

    [BurstCompile]
    public partial struct MeteorCreatorS : ISystem
    {
        // Максимальное количество точек
        public const int MAX_POINTS = 1000000; 

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<MeteorCreatedTag>())
                return;
            
            float3 center = new float3(0, 0, 0);

            // Создаем NativeList для результата (плоский список узлов)
            NativeList<int3> resultNodes = new NativeList<int3>(MAX_POINTS, Allocator.TempJob);
            NativeList<int2> processedNodes = new NativeList<int2>(MAX_POINTS, Allocator.TempJob);

            // Массив направлений
            NativeArray<float3> directions = new NativeArray<float3>(6, Allocator.TempJob);
            directions[0] = new float3(1, 0, 0);
            directions[1] = new float3(-1, 0, 0);
            directions[2] = new float3(0, 1, 0);
            directions[3] = new float3(0, -1, 0);
            directions[4] = new float3(0, 0, 1);
            directions[5] = new float3(0, 0, -1);

            foreach (var prefabBuffer in SystemAPI.Query<DynamicBuffer<PrefabBufferElement>>())
            {
                if (prefabBuffer.IsEmpty)
                    continue;

                EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.World.Unmanaged);

                Entity baseCube = Entity.Null;
                Entity sandCube = Entity.Null;
                Entity rockCube = Entity.Null;

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

                // Создаем центральный блок вручную
                CreateCube(center, baseCube, ecb);
                
                processedNodes.AddNoResize(new int2(0, 1));
                processedNodes.AddNoResize(new int2(1, 0));
                processedNodes.AddNoResize(new int2(2, 0));
                processedNodes.AddNoResize(new int2(3, 0));
                processedNodes.AddNoResize(new int2(4, 0));

                int maxRadius = 10;
                int maxNeighbors = 5;
                int minNeighbors = 2;
                uint seed = 1;

                // Создаем Job
                var job = new MeteorGenerationTreeJob
                {
                    maxRadius = maxRadius, // Глубина генерации
                    minNeighbors = minNeighbors, // Минимальное число соседей
                    maxNeighbors = maxNeighbors, // Максимальное число соседей
                    seed = seed,
                    directions = directions,
                    resultNodes = resultNodes.AsParallelWriter(),
                    processedNodes = processedNodes,
                };

                // Запускаем Job
                JobHandle jobHandle = job.Schedule(processedNodes.Length, 1); // Параллельное выполнение
                jobHandle.Complete();

                Debug.LogError("Completed!");

               // Преобразуем плоский список в координаты
                NativeArray<float3> resultPoints = new NativeArray<float3>(resultNodes.Length, Allocator.TempJob);
                for (int i = 0; i < resultNodes.Length; i++)
                {
                    resultPoints[i] = resultNodes[i].position;
                }

                Debug.Break();
                /*for (int i = 0; i < resultPoints.Length; ++i)
                {
                    CreateCube(resultPoints[i], sandCube, ecb);
                }*/

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


                Debug.LogError("Completed2!");

                Entity markerEntity = ecb.CreateEntity();
                ecb.AddComponent<MeteorCreatedTag>(markerEntity);

                // Освобождаем память
                directions.Dispose();
                resultNodes.Dispose();
                processedNodes.Dispose();

                break;
            }
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
        }
    }

    [BurstCompile]
    public struct SpawnCubesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> positions; // Список координат

        public EntityCommandBuffer.ParallelWriter ecb; // EntityCommandBuffer для записи команд
        public Entity cubePrefab; // Префаб куба

        public void Execute(int index)
        {
            // Создаем сущность куба
            Entity cubeEntity = ecb.Instantiate(0, cubePrefab);

            // Устанавливаем позицию куба
            ecb.SetComponent(index, cubeEntity, new LocalTransform
            {
                Position = positions[index],
                Rotation = quaternion.identity,
                Scale = 1f
            });
        }
    }
   
    [BurstCompile]
    public struct MeteorGenerationTreeJob : IJobParallelFor
    {
        public int maxRadius;
        public int minNeighbors;
        public int maxNeighbors;
        public uint seed;
        public int parent;

        [ReadOnly] public NativeArray<float3> directions; // Массив направлений

        [NativeDisableParallelForRestriction]
        public NativeList<int3>.ParallelWriter resultNodes; // Результат (плоский список узлов)
        
        [NativeDisableParallelForRestriction]
        public NativeList<int2> processedNodes; // Результат (плоский список узлов)

        public void Execute(int index)
        {
            int currentNode = GetFreeNode();

            if (currentNode == -1)
                return;

            Random random = new Random(seed + (uint)index);
            int neighborsCount = random.NextInt(minNeighbors, maxNeighbors);
            
            int[] neighbors = new int[neighborsCount];
            int counter = 0;
            int newIndex = 0;
            while(counter < neighborsCount) // find random neighbors
            {
                newIndex = random.NextInt(0, directions.Length);
                for(int i = 0; i < counter; i++)
                    if (newIndex == neighbors[i])
                    {
                        newIndex = -1;
                        break;
                    }

                if (newIndex != -1)
                {
                    neighbors[counter] = newIndex;
                    counter++;
                }
            }

            int newPoint = 0;
            for (int i = 0; i < neighbors.Length; i++)// Добавляем узел в плоский список
            {
                newPoint = GetNewPoint();
                
                int3 point = new int3(currentNode, newPoint, i);
                resultNodes.AddNoResize(point);
                Debug.LogError($"Added node: {point}");
            }
        }

        private int GetNewPoint()
        {
            int preValue = 0;
            for (int i = 0; i < processedNodes.Length; i++)
            {
                if (processedNodes[i].x > 0)
                    preValue = processedNodes[i].x;
                else
                {
                    processedNodes.AddNoResize(new int2(preValue + 1, 0));
                    break;
                }
            }

            return preValue + 1;
        }

        private int GetFreeNode()
        {
            for (int i = 0; i < processedNodes.Length; i++)
            {
                if (processedNodes[i].x != 0 && processedNodes[i].y == 0)
                {
                    processedNodes[i] = new int2(i, 1);
                    return i;
                }
            }

            return -1;
        }
    }*/

    /*[BurstCompile]
    public partial struct MeteorCreatorS : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<MeteorCreatedTag>())
                return;

            NativeArray<float3> directions = new NativeArray<float3>(6, Unity.Collections.Allocator.TempJob);

            directions[0] = new float3(1, 0, 0);
            directions[1] = new float3(-1, 0, 0);
            directions[2] = new float3(0, 1, 0);
            directions[3] = new float3(0, -1, 0);
            directions[4] = new float3(0, 0, 1);
            directions[5] = new float3(0, 0, -1);


            foreach (var prefabBuffer in SystemAPI.Query<DynamicBuffer<PrefabBufferElement>>())
            {
                if (prefabBuffer.IsEmpty)
                    continue;

                EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.World.Unmanaged);

                Entity baseCube = Entity.Null;
                Entity sandCube = Entity.Null;
                Entity rockCube = Entity.Null;

                foreach(var c in prefabBuffer)
                {
                    if(c.cubeType == CubeType.Resource)
                    {
                        baseCube = c.Prefab;
                    }
                    else if(c.cubeType == CubeType.Sand)
                    {
                        sandCube = c.Prefab;
                    }
                    else if (c.cubeType == CubeType.Rock)
                    {
                        rockCube = c.Prefab;
                    }

                    if(baseCube != Entity.Null && sandCube != Entity.Null && rockCube != Entity.Null)
                    {
                        break;
                    }
                }

                NativeList<float3> queue = new NativeList<float3>(Allocator.TempJob);
                NativeHashMap<float3, bool> points = new NativeHashMap<float3, bool>(1000, Allocator.TempJob);

                float3 center = new float3(0, 0, 0);
                int maxPoints = 500000;
                int maxRadius = 10;
                int maxNeighbors = 5;
                int minNeighbors = 2;
                uint seed = 1;
                float neighborProbability = 0.8f;
                NativeArray<int> pointCount = new NativeArray<int>(1, Allocator.TempJob);
                NativeList<float3> result = new NativeList<float3>(maxPoints, Allocator.TempJob);
                CreateCube(center, baseCube, ecb);

                NativeHashMap<float3, bool> occupiedCells = new NativeHashMap<float3, bool>(maxPoints, Allocator.TempJob);

                queue.Add(center);
                points.TryAdd(center, true);

                var job = new MeteorGeneration4Job
                {
                    center = center,
                    maxRadius = maxRadius,
                    minNeighbors = minNeighbors,
                    maxNeighbors = maxNeighbors,
                    neighborProbability = neighborProbability,
                    seed = seed,
                    occupiedCells = occupiedCells,
                    result = result.AsParallelWriter(),
                    directions = directions,
                };

                JobHandle jobHandle = job.Schedule(maxPoints, 64); // Параллельное выполнение
                jobHandle.Complete();

                for (int i = 0; i < result.Length; ++i)
                {
                    CreateCube(result[i], sandCube, ecb);
                }


                Entity markerEntity = ecb.CreateEntity();
                ecb.AddComponent<MeteorCreatedTag>(markerEntity);

                queue.Dispose();
                points.Dispose();
                result.Dispose();
                pointCount.Dispose();

                break;
            }
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
        }
    }*/

    /*
    [BurstCompile]
    public struct MeteorGeneration4Job : IJobParallelFor
    {
        public float3 center;
        public int maxRadius;
        public int minNeighbors;
        public int maxNeighbors;
        public float neighborProbability;
        public uint seed;
        public NativeArray<float3> directions;

        [NativeDisableParallelForRestriction]
        public NativeHashMap<float3, bool> occupiedCells; // Занятые ячейки

        [NativeDisableParallelForRestriction]
        public NativeList<float3>.ParallelWriter result; // Результат (список точек)

        public void Execute(int index)
        {
            Random random = new Random(seed + (uint)index);

            // Направления для соседей (6 граней куба)


            // Текущая точка
            float3 current = center;

            // Генерация ветви
            for (int depth = 0; depth < maxRadius; depth++)
            {
                // Выбираем случайное направление
                float3 direction = directions.ElementAt(random.NextInt(0, directions.Length));
                float3 neighbor = current + direction;

                // Проверяем, что точка находится в пределах радиуса
                if (math.distancesq(neighbor, center) > maxRadius * maxRadius)
                    break;

                // Проверяем, что ячейка свободна
                if (!occupiedCells.ContainsKey(neighbor))
                {
                    // Считаем количество соседей
                    int neighborCount = 0;
                    foreach (var dir in directions)
                    {
                        float3 adjacent = neighbor + dir;
                        if (occupiedCells.ContainsKey(adjacent))
                            neighborCount++;
                    }

                    // Проверяем, что количество соседей находится в пределах minNeighbors и maxNeighbors
                    if (neighborCount >= minNeighbors && neighborCount <= maxNeighbors)
                    {
                        // Помечаем ячейку как занятую
                        occupiedCells.TryAdd(neighbor, true);

                        // Добавляем точку в результат
                        result.AddNoResize(neighbor);

                        // Переходим к следующей точке
                        current = neighbor;
                    }
                    else
                    {
                        // Если количество соседей не соответствует требованиям, прерываем ветвь
                        break;
                    }
                }
                else
                {
                    // Если ячейка занята, прерываем ветвь
                    break;
                }
            }
        }
    }


    [BurstCompile]
    public struct MeteorGeneration3Job : IJobParallelFor
    {
        public float3 center;
        public int maxRadius;
        public int minNeighbors;
        public int maxNeighbors;
        public float neighborProbability;
        public uint seed;

        [NativeDisableParallelForRestriction]
        public NativeArray<float3> result; // Результат (список точек)
        [NativeDisableParallelForRestriction]
        public NativeArray<int> pointCount; // Количество сгенерированных точек

        public void Execute(int index)
        {
            Random random = new Random(seed + (uint)index);

            // Направления для соседей (6 граней куба)
            float3[] directions = new float3[]
        {
            new float3(1, 0, 0),
            new float3(-1, 0, 0),
            new float3(0, 1, 0),
            new float3(0, -1, 0),
            new float3(0, 0, 1),
            new float3(0, 0, -1)
        };

            // Локальный список для хранения точек
            NativeList<float3> localPoints = new NativeList<float3>(Allocator.Temp);

            // Текущая точка
            float3 current = center;

            // Генерация ветви
            for (int depth = 0; depth < maxRadius; depth++)
            {
                // Выбираем случайное направление
                float3 direction = directions[random.NextInt(0, directions.Length)];
                float3 neighbor = current + direction;

                // Проверяем, что точка находится в пределах радиуса
                if (math.distancesq(neighbor, center) > maxRadius * maxRadius)
                    break;

                // Добавляем точку в локальный список
                localPoints.Add(neighbor);

                // Переходим к следующей точке
                current = neighbor;
            }

            // Атомарно увеличиваем счетчик точек
            int startIndex = pointCount[0] + localPoints.Length;

            // Копируем локальные точки в результат
            for (int i = 0; i < localPoints.Length; i++)
            {
                result[startIndex + i] = localPoints[i];
            }

            // Освобождаем локальный список
            localPoints.Dispose();
        }
    }

    [BurstCompile]
    public struct MeteorGeneration2Job : IJobParallelFor
    {
        public float3 center;
        public int maxRadius;
        public int minNeighbors;
        public int maxNeighbors;
        public float neighborProbability;
        public uint seed;

        public NativeArray<float3> result; // Результат (список точек)
        public NativeArray<int> pointCount; // Количество сгенерированных точек

        public void Execute(int index)
        {
            Random random = new Random(seed + (uint)index);

            // Направления для соседей (6 граней куба)
            float3[] directions = new float3[]
        {
            new float3(1, 0, 0),
            new float3(-1, 0, 0),
            new float3(0, 1, 0),
            new float3(0, -1, 0),
            new float3(0, 0, 1),
            new float3(0, 0, -1)
        };

            // Текущая точка
            float3 current = center;

            // Генерация ветви
            for (int depth = 0; depth < maxRadius; depth++)
            {
                // Выбираем случайное направление
                float3 direction = directions[random.NextInt(0, directions.Length)];
                float3 neighbor = current + direction;

                // Проверяем, что точка находится в пределах радиуса
                if (math.distance(neighbor, center) > maxRadius)
                    break;

                // Атомарно увеличиваем счетчик точек
                int pointIndex = pointCount[0] - 1;

                // Проверяем, что не превышен лимит точек
                if (pointIndex >= result.Length)
                    break;

                // Добавляем точку в результат
                result[pointIndex] = neighbor;

                // Переходим к следующей точке
                current = neighbor;
            }
        }
    }

    [BurstCompile]
    public struct MeteorGenerationJob : IJob
    {
        public float3 center;
        public int maxRadius;
        public int minNeighbors;
        public int maxNeighbors;
        public float neighborProbability;
        public uint seed;

        public NativeList<float3> queue; // Список точек для обработки
        public NativeHashMap<float3, bool> points; // Хранилище уникальных точек
        public NativeList<float3> result; // Результат (список точек)

        public void Execute()
        {
            Random random = new Random(seed);

            // Направления для соседей (6 граней куба)
            float3[] directions = new float3[]
        {
            new float3(1, 0, 0),
            new float3(-1, 0, 0),
            new float3(0, 1, 0),
            new float3(0, -1, 0),
            new float3(0, 0, 1),
            new float3(0, 0, -1)
        };

            // Индекс для обработки очереди
            int index = 0;

            // BFS для генерации точек
            while (index < queue.Length)
            {
                float3 current = queue[index];
                index++;

                // Считаем количество соседей для текущей точки
                int neighborCount = 0;

                // Перебираем всех возможных соседей
                foreach (var dir in directions)
                {
                    float3 neighbor = current + dir;

                    // Проверяем, что точка находится в пределах радиуса
                    if (math.distance(neighbor, center) > maxRadius)
                        continue;

                    // Проверяем, что точка еще не была добавлена
                    if (!points.ContainsKey(neighbor))
                    {
                        // Случайно решаем, добавлять ли соседа
                        if (random.NextFloat() < neighborProbability)
                        {
                            points.TryAdd(neighbor, true);
                            queue.Add(neighbor); // Добавляем соседа в очередь
                            neighborCount++;
                        }
                    }
                }

                // Если количество соседей меньше minNeighbors, точка не будет дальше ветвиться
                if (neighborCount < minNeighbors)
                {
                    // Удаляем точку из очереди, чтобы она не ветвилась дальше
                    queue.RemoveRange(index - 1, 1);
                    index--;
                }
            }

            // Преобразуем NativeHashMap в NativeList
            foreach (var point in points)
            {
                result.Add(point.Key);
            }
        }
    }*/
}