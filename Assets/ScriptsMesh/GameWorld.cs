using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDB.Meshes
{
    public class GameWorld : MonoBehaviour
    {
        public Dictionary<Vector2Int, ChunkData> ChunkDatas = new Dictionary<Vector2Int, ChunkData>();
        public ChunkRenderer ChunkRendererPrefab;

        private void Start()
        {
            for(int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
            {
                var xPos = x * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;
                var zPos = y * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;
                var chunkData = new ChunkData();
                
                chunkData.Pos = new Vector2Int() {x = x, y = y};
                
                chunkData.Blocks = TerrainGenerator
                    .GenerateTerrain(xPos, zPos).Blocks;
                
                ChunkDatas.Add(new Vector2Int(x, y), chunkData);
                
                var chunk = Instantiate(ChunkRendererPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity, transform);
                chunk.CData = chunkData;
                chunk.World = this;
            }
        }
    }
}