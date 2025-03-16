using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDB.Meshes
{
    public static class MeshBuilder
    {
        public const int textureWidth = 1972;
        public const int textureHeight = 916;
        public const int textureSize = 34;

        public const int ChunkWidth = 16;
        public const int ChunkWidthSq = ChunkWidth * ChunkWidth;
        public const int ChunkHeight = 128;
        public const float BlockScale = .25f;

        private static Dictionary<BlockType, BlockInfoSimple> BlocksDict = null;
        private static BlockInfoSimple _defaultBlock = null;

        public static void SetConfig(Dictionary<BlockType, BlockInfoSimple> config, BlockInfoSimple defaultBlock)
        {
            BlocksDict = config;
            _defaultBlock = defaultBlock;
        }
        
        public static GameWorld.GeneratedMeshData GenMeshData(ChunkData chunkData)
        {
            List<GameWorld.GeneratedMeshVertex> verticies = new();
            
            int maxY = 0;
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int x = 0; x < ChunkWidth; x++)
                {
                    for (int z = 0; z < ChunkWidth; z++)
                    {
                        if (TryGenBlock(x, y, z, verticies, chunkData))
                        {
                            if (maxY < y)
                                maxY = y;
                        }
                    }
                }
            }
            
            var mesh = new GameWorld.GeneratedMeshData();
            mesh.Vertices = verticies.ToArray();
            
            Vector3 boundsSize = new Vector3(ChunkWidth, maxY, ChunkWidth) * BlockScale;
            mesh.Bounds = new Bounds(boundsSize/2, boundsSize);
            mesh.ChunkData = chunkData;
            return mesh;
        }

        private static bool TryGenBlock(int x, int y, int z, List<GameWorld.GeneratedMeshVertex> verticies, ChunkData chunkData)
        {
            var pos = new Vector3Int(x, y, z);
            var blockType = GetBlockInPos(pos, chunkData);

            if (GetBlockInPos(pos, chunkData) == 0)
                return false;

            if (GetBlockInPos(pos + Vector3Int.right, chunkData) == 0)
            {
                GenRightSide(pos, verticies, blockType);
            }

            if (GetBlockInPos(pos + Vector3Int.left, chunkData) == 0)
            {
                GenLeftSide(pos, verticies, blockType);
            }

            if (GetBlockInPos(pos + Vector3Int.forward, chunkData) == 0)
            {
                GenFrontSide(pos, verticies, blockType);
            }

            if (GetBlockInPos(pos + Vector3Int.back, chunkData) == 0)
            {
                GenBackSide(pos, verticies, blockType);
            }

            if (GetBlockInPos(pos + Vector3Int.up, chunkData) == 0)
            {
                GenTopSide(pos, verticies, blockType);
            }

            if ( /*pos.y > 0 && */GetBlockInPos(pos + Vector3Int.down, chunkData) == 0)
            {
                GenBottomSide(pos, verticies, blockType);
            }

            return true;
        }

        private static BlockType GetBlockInPos(Vector3Int pos, ChunkData chunkData)
        {
            if (pos.x >= 0 && pos.x < ChunkWidth &&
                pos.y >= 0 && pos.y < ChunkHeight &&
                pos.z >= 0 && pos.z < ChunkWidth)

            {
                int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                return chunkData.Blocks[index];
            }
            else
            {
                if (pos.y < 0 || pos.y >= ChunkHeight)
                    return BlockType.Air;

                if (pos.x < 0)
                {
                    if (chunkData.LeftChunk == null)
                        return BlockType.Air;

                    pos.x += ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return chunkData.LeftChunk.Blocks[index];
                }

                if (pos.x >= ChunkWidth)
                {
                    if (chunkData.RightChunk == null)
                        return BlockType.Air;

                    pos.x -= ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return chunkData.RightChunk.Blocks[index];
                }

                if (pos.z < 0)
                {
                    if (chunkData.BackChunk == null)
                        return BlockType.Air;

                    pos.z += ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return chunkData.BackChunk.Blocks[index];
                }

                if (pos.z >= ChunkWidth)
                {
                    if (chunkData.FwdChunk == null)
                        return BlockType.Air;

                    pos.z -= ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return chunkData.FwdChunk.Blocks[index];
                }

                return BlockType.Air;
            }
        }

        private static void GenRightSide(Vector3Int pos, List<GameWorld.GeneratedMeshVertex> vertices, BlockType btype)
        {
            GameWorld.GeneratedMeshVertex vertex = new GameWorld.GeneratedMeshVertex();

            vertex.normalX = sbyte.MaxValue;
            vertex.normalY = 0;
            vertex.normalZ = 0;
            vertex.normalW = 1;
            
            GetUVs(btype, Vector3Int.right, out vertex.uvX, out vertex.uvY);
            
            vertex.pos = (new Vector3(1, 0, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 1, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 0, 1) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 1, 1) + pos) * BlockScale;
            vertices.Add(vertex);
        }

        private static void GenLeftSide(Vector3Int pos, List<GameWorld.GeneratedMeshVertex> vertices, BlockType btype)
        {
            GameWorld.GeneratedMeshVertex vertex = new GameWorld.GeneratedMeshVertex();

            vertex.normalX = -sbyte.MaxValue;
            vertex.normalY = 0;
            vertex.normalZ = 0;
            vertex.normalW = 1;
            
            GetUVs(btype, Vector3Int.left, out vertex.uvX, out vertex.uvY);
            
            vertex.pos = (new Vector3(0, 0, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(0, 0, 1) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(0, 1, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(0, 1, 1) + pos) * BlockScale;
            vertices.Add(vertex);
        }

        private static void GenFrontSide(Vector3Int pos, List<GameWorld.GeneratedMeshVertex> vertices, BlockType btype)
        {
            GameWorld.GeneratedMeshVertex vertex = new GameWorld.GeneratedMeshVertex();

            vertex.normalX = 0;
            vertex.normalY = 0;
            vertex.normalZ = sbyte.MaxValue;
            vertex.normalW = 1;
            
            GetUVs(btype, Vector3Int.forward, out vertex.uvX, out vertex.uvY);
            
            vertex.pos = (new Vector3(0, 0, 1) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 0, 1) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(0, 1, 1) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 1, 1) + pos) * BlockScale;
            vertices.Add(vertex);
        }

        private static void GenBackSide(Vector3Int pos, List<GameWorld.GeneratedMeshVertex> vertices, BlockType btype)
        {
            GameWorld.GeneratedMeshVertex vertex = new GameWorld.GeneratedMeshVertex();

            vertex.normalX = 0;
            vertex.normalY = 0;
            vertex.normalZ = -sbyte.MaxValue;
            vertex.normalW = 1;
            
            GetUVs(btype, Vector3Int.back, out vertex.uvX, out vertex.uvY);
            
            vertex.pos = (new Vector3(0, 0, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(0, 1, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 0, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 1, 0) + pos) * BlockScale;
            vertices.Add(vertex);
        }

        private static void GenTopSide(Vector3Int pos, List<GameWorld.GeneratedMeshVertex> vertices, BlockType btype)
        {
            GameWorld.GeneratedMeshVertex vertex = new GameWorld.GeneratedMeshVertex();

            vertex.normalX = 0;
            vertex.normalY = sbyte.MaxValue;
            vertex.normalZ = 0;
            vertex.normalW = 1;
            
            GetUVs(btype, Vector3Int.up, out vertex.uvX, out vertex.uvY);
            
            vertex.pos = (new Vector3(0, 1, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(0, 1, 1) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 1, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 1, 1) + pos) * BlockScale;
            vertices.Add(vertex);
        }

        private static void GenBottomSide(Vector3Int pos, List<GameWorld.GeneratedMeshVertex> vertices, BlockType btype)
        {
            GameWorld.GeneratedMeshVertex vertex = new GameWorld.GeneratedMeshVertex();

            vertex.normalX = 0;
            vertex.normalY = -sbyte.MaxValue;
            vertex.normalZ = 0;
            vertex.normalW = 1;
            
            GetUVs(btype, Vector3Int.down, out vertex.uvX, out vertex.uvY);
            
            vertex.pos = (new Vector3(0, 0, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 0, 0) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(0, 0, 1) + pos) * BlockScale;
            vertices.Add(vertex);
            vertex.pos = (new Vector3(1, 0, 1) + pos) * BlockScale;
            vertices.Add(vertex);
        }

        private static void GetUVs(BlockType blockType, Vector3Int normal, out ushort x, out ushort y)
        {
            BlockInfoSimple bInfoSimple = _defaultBlock;
            BlocksDict.TryGetValue(blockType, out _defaultBlock);
            
            var offset = bInfoSimple.GetPixelOffset(normal);
            x = unchecked((ushort)(offset.x * textureSize * textureWidth));
            y = unchecked((ushort)(offset.y * textureSize * textureHeight));
        }
    }
}