using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace GDB.Meshes
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        [SerializeField] private BlocksCommon _config = null;
        
        public int textureWidth = 1972;
        public int textureHeight = 916;
        public int textureSize = 34;
        
        public const int ChunkWidth = 32;
        public const int ChunkWidthSq = ChunkWidth * ChunkWidth;
        public const int ChunkHeight = 128;
        public const float BlockScale = 1f;

        public ChunkData CData = null;
        public GameWorld World = null;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();

        private Mesh chunkMesh;

        private ChunkData _leftChunk;
        private ChunkData _rightChunk;
        private ChunkData _fwdChunk;
        private ChunkData _backChunk;

        private static int[] _triangles;

        private void Start()
        {
            World.ChunkDatas.TryGetValue(CData.Pos + Vector2Int.left, out _leftChunk);
            World.ChunkDatas.TryGetValue(CData.Pos + Vector2Int.right, out _rightChunk);
            World.ChunkDatas.TryGetValue(CData.Pos + Vector2Int.up, out _fwdChunk);
            World.ChunkDatas.TryGetValue(CData.Pos + Vector2Int.down, out _backChunk);
            
            chunkMesh = new Mesh();
            RegenerateMesh();

            GetComponent<MeshFilter>().mesh = chunkMesh;
        }

        public static void InitTriangles()
        {
            _triangles = new int[65536*6/4];

            int vertNum = 4;
            for (int i = 0; i < _triangles.Length; i+=6)
            {
                _triangles[i] = vertNum - 4;
                _triangles[i + 1] = vertNum - 3;
                _triangles[i + 2] = vertNum - 2;
                 
                _triangles[i + 3] = vertNum - 3;
                _triangles[i + 4] = vertNum - 1;
                _triangles[i + 5] = vertNum - 2;
                vertNum += 4;
            }
            
        }
        
        private static ProfilerMarker _genMarker = new ProfilerMarker(ProfilerCategory.Loading, "GenMesh");
        private void RegenerateMesh()
        {
            _genMarker.Begin();
            
            vertices.Clear();
            uvs.Clear();

            int maxY = 0;
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int x = 0; x < ChunkWidth; x++)
                {
                    for (int z = 0; z < ChunkWidth; z++)
                    {
                        if (TryGenBlock(x, y, z))
                        {
                            if(maxY < y)
                                maxY = y;
                        }
                    }
                }
            }

            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.SetTriangles(_triangles, 0, vertices.Count * 6 / 4, 0, false);
            
            chunkMesh.Optimize();
            
            chunkMesh.RecalculateNormals();
            Vector3 bSize = new Vector3(ChunkWidth, maxY, ChunkWidth) * BlockScale;
            chunkMesh.bounds = new Bounds(bSize/2, bSize);
            
            GetComponent<MeshCollider>().sharedMesh = chunkMesh;
            
            _genMarker.End();
        }

        public void SpawnBlock(Vector3Int pos)
        {
            int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
            CData.Blocks[index] = BlockType.Dirt;
            RegenerateMesh();
        }
        
        public void DestroyBlock(Vector3Int pos)
        {
            int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
            CData.Blocks[index] = BlockType.Air;
            RegenerateMesh();
        }
        
        private bool TryGenBlock(int x, int y, int z)
        {
            var pos = new Vector3Int(x, y, z);
            var blockType = GetBlockInPos(pos);
            
            if(GetBlockInPos(pos) == 0) 
                return false;
            
            if(GetBlockInPos(pos + Vector3Int.right) == 0)
            {
                GenRightSide(pos);
                AddUVs(blockType, Vector2Int.right);
            }
            if(GetBlockInPos(pos + Vector3Int.left) == 0)
            {
                GenLeftSide(pos);
                AddUVs(blockType, Vector2Int.left);
            }
            if(GetBlockInPos(pos + Vector3Int.forward) == 0)
            {
                GenFrontSide(pos);
                AddUVs(blockType, (Vector2Int)Vector3Int.forward);
            }
            if(GetBlockInPos(pos + Vector3Int.back) == 0)
            {
                GenBackSide(pos);
                AddUVs(blockType, (Vector2Int)Vector3Int.back);
            }
            if(GetBlockInPos(pos + Vector3Int.up) == 0)
            {
                GenTopSide(pos);
                AddUVs(blockType, Vector2Int.up);
            }
            if(/*pos.y > 0 && */GetBlockInPos(pos + Vector3Int.down) == 0)
            {
                GenBottomSide(pos);
                AddUVs(blockType, Vector2Int.down);
            }

            return true;
        }

        private BlockType GetBlockInPos(Vector3Int pos)
        {
            if(pos.x >= 0 && pos.x < ChunkWidth &&
               pos.y >= 0 && pos.y < ChunkHeight &&
               pos.z >= 0 && pos.z < ChunkWidth)

            {
                int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                return CData.Blocks[index];
            }
            else
            {
                if (pos.y < 0 || pos.y >= ChunkHeight) 
                    return BlockType.Air;
                
                if (pos.x < 0)
                {
                    if (_leftChunk == null)
                        return BlockType.Air;
                    
                    pos.x += ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return _leftChunk.Blocks[index];
                }
                
                if (pos.x >= ChunkWidth)
                {
                    if (_rightChunk == null)
                        return BlockType.Air;
                    
                    pos.x -= ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return _rightChunk.Blocks[index];
                }
                
                if (pos.z < 0)
                {
                    if (_backChunk == null)
                        return BlockType.Air;
                    
                    pos.z += ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return _backChunk.Blocks[index];
                }
                
                if (pos.z >= ChunkWidth)
                {
                    if (_fwdChunk == null)
                        return BlockType.Air;
                    
                    pos.z -= ChunkWidth;
                    int index = pos.x + pos.y * ChunkWidthSq + pos.z * ChunkWidth;
                    return _fwdChunk.Blocks[index];
                }
                
                return BlockType.Air;
            }
        }
        
        private void AddUVs(BlockType blockType, Vector2Int normal)
        {
            BlockInfoSimple bInfoSimple = _config.Get(blockType);
            var offset = bInfoSimple.GetPixelOffset(normal);
            Vector2 uv = new Vector2((offset.x * textureSize) / textureWidth, (offset.y * textureSize) / textureHeight);

            for (int i = 0; i < 4; i++)
            {
                uvs.Add(uv);
            }
        }

        private void GenRightSide(Vector3Int pos)
        {
            vertices.Add(new Vector3(1, 0, 0)+ pos);
            vertices.Add(new Vector3(1, 1, 0)+ pos);
            vertices.Add(new Vector3(1, 0, 1)+ pos);
            vertices.Add(new Vector3(1, 1, 1)+ pos);
        }
        
        private void GenLeftSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 1)+ pos) * BlockScale);
        }
        
        private void GenFrontSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 1)+ pos) * BlockScale);
        }
        
        private void GenBackSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 0) + pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 0)+ pos) * BlockScale);
        }
        
        private void GenTopSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 1, 0) + pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 1)+ pos) * BlockScale);
        }
        
        private void GenBottomSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 0) + pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 1)+ pos) * BlockScale);
        }
    }
}