using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace GDB.Planetary
{
    public class CubesBlobAssetConstructorA: MonoBehaviour
    {
        public CubeGOInfo[] CubePrefabs;

        public class Baker : Baker<CubesBlobAssetConstructorA>
        {
            public override void Bake(CubesBlobAssetConstructorA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<PrefabBufferElement>(entity);

                foreach (var prefab in authoring.CubePrefabs)
                {
                    buffer.Add(new PrefabBufferElement
                    {
                        Prefab = GetEntity(prefab.cubeGO, TransformUsageFlags.Dynamic),
                        cubeType = prefab.cubeType,
                    });
                }
            }
        }       
    }

    public struct PrefabBufferElement : IBufferElementData
    {
        public Entity Prefab;
        public CubeType cubeType;
    }

    [Serializable]
    public struct CubeGOInfo
    {
        public GameObject cubeGO;
        public CubeType cubeType;
    }    
}