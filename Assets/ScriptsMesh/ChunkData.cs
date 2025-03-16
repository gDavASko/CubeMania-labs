using UnityEngine;

namespace GDB.Meshes
{
    public class ChunkData
    {
        public ChunkDataState State;
        
        public Vector2Int Pos;
        public ChunkRenderer Renderer;
        public BlockType[] Blocks;
        
        public ChunkData LeftChunk;
        public ChunkData RightChunk;
        public ChunkData FwdChunk;
        public ChunkData BackChunk;
    }

    public enum ChunkDataState : byte
    {
        StartedLoading = 0,
        Loaded = 1,
        StartedMeshing = 2,
        SpawnedInWorld = 4,
        Unloaded = 8,
    }
}