using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDB.Meshes
{
    public enum BlockType: byte
    {
        Air = 0,
        Stone = 1,
        Grass = 2,
        Sand = 4,
        Dirt = 8,
        Water = 16,
        Bedrock = 32,
    }
    
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        public int textureWidth = 1972;
        public int textureHeight = 916;
        public int textureSize = 34;
        
        public const int ChunkWidth = 32;
        public const int ChunkHeight = 128;
        public const float BlockScale = 1f;

        public ChunkData CData = null;
        public GameWorld World = null;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();

        private Mesh chunkMesh;

        private void Start()
        {
            chunkMesh = new Mesh();
            RegenerateMesh();

            GetComponent<MeshFilter>().mesh = chunkMesh;
        }

        private void RegenerateMesh()
        {
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
            
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int x = 0; x < ChunkWidth; x++)
                {
                    for (int z = 0; z < ChunkWidth; z++)
                    {
                        GenBlock(x, y, z);
                    }
                }
            }

            chunkMesh.triangles = Array.Empty<int>();
            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            
            chunkMesh.Optimize();
            
            chunkMesh.RecalculateNormals();
            chunkMesh.RecalculateBounds();
            
            GetComponent<MeshCollider>().sharedMesh = chunkMesh;
        }

        public void SpawnBlock(Vector3Int pos)
        {
            CData.Blocks[pos.x, pos.y, pos.z] = BlockType.Dirt;
            RegenerateMesh();
        }
        
        public void DestroyBlock(Vector3Int pos)
        {
            CData.Blocks[pos.x, pos.y, pos.z] = BlockType.Air;
            RegenerateMesh();
        }
        
        private void GenBlock(int x, int y, int z)
        {
            var pos = new Vector3Int(x, y, z);
            var blockType = GetBlockInPos(pos);
            
            if(GetBlockInPos(pos) == 0) return;
            
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
            if(GetBlockInPos(pos + Vector3Int.down) == 0)
            {
                GenBottomSide(pos);
                AddUVs(blockType, Vector2Int.down);
            }
        }

        private BlockType GetBlockInPos(Vector3Int pos)
        {
            if(pos.x >= 0 && pos.x < ChunkWidth &&
               pos.y >= 0 && pos.y < ChunkHeight &&
               pos.z >= 0 && pos.z < ChunkWidth)

            {
                return CData.Blocks[pos.x, pos.y, pos.z];
            }
            else
            {
                if (pos.y < 0 || pos.y >= ChunkHeight) 
                    return BlockType.Air;
                
                var adjCPos = CData.Pos;
                if (pos.x < 0)
                {
                    adjCPos.x--;
                    pos.x += ChunkWidth;
                }
                else if (pos.x >= ChunkWidth)
                {
                    adjCPos.x++;
                    pos.x -= ChunkWidth;
                }
                
                if (pos.z < 0)
                {
                    adjCPos.y--;
                    pos.z += ChunkWidth;
                }
                else if (pos.z >= ChunkWidth)
                {
                    adjCPos.y++;
                    pos.z -= ChunkWidth;
                }

                if(World.ChunkDatas.TryGetValue(adjCPos, out var chunk))
                {
                    return chunk.Blocks[pos.x, pos.y, pos.z];
                }
                
                return BlockType.Air;
            }
        }
        
        private void AddLastVertSquare()
        {
            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);
            
            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 1);
            triangles.Add(vertices.Count - 2);
        }

        /*private void AddUVs(BlockType type, Vector2Int normal)
        {
            uvs.Add(new Vector2((7f * textureSize) / textureWidth, (26f * textureSize) / textureHeight));
            uvs.Add(new Vector2((7f * textureSize) / textureWidth, (26f * textureSize) / textureHeight));
            uvs.Add(new Vector2((7f * textureSize) / textureWidth, (26f * textureSize) / textureHeight));
            uvs.Add(new Vector2((7f * textureSize) / textureWidth, (26f * textureSize) / textureHeight));
        }*/
        
        private void AddUVs(BlockType blockType, Vector2Int normal)
        {
            Vector2 uv;
            if (blockType == BlockType.Grass)
            {
                uv = normal == Vector2Int.up ? new Vector2((23f * textureSize) / textureWidth, (10f * textureSize) / textureHeight) :
                    normal == Vector2Int.down ? new Vector2((23f * textureSize) / textureWidth, (10f * textureSize) / textureHeight) :
                    new Vector2((23f * textureSize) / textureWidth, (10f * textureSize) / textureHeight);
            }
            else if (blockType == BlockType.Stone)
            {
                uv = new Vector2((1f * textureSize) / textureWidth, (9f * textureSize) / textureHeight);
            }
            else if (blockType == BlockType.Dirt)
            {
                uv = new Vector2((14f * textureSize) / textureWidth, (7f * textureSize) / textureHeight);
            }
            else
            {
                uv = new Vector2((0f * textureSize) / textureWidth, (1f * textureSize) / textureHeight);
            }

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

            AddLastVertSquare();
        }
        
        private void GenLeftSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 1)+ pos) * BlockScale);

            AddLastVertSquare();
        }
        
        private void GenFrontSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 1)+ pos) * BlockScale);
            

            AddLastVertSquare();
        }
        
        private void GenBackSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 0) + pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 0)+ pos) * BlockScale);

            AddLastVertSquare();
        }
        
        private void GenTopSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 1, 0) + pos) * BlockScale);
            vertices.Add((new Vector3(0, 1, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 1, 1)+ pos) * BlockScale);

            AddLastVertSquare();
        }
        
        private void GenBottomSide(Vector3Int pos)
        {
            vertices.Add((new Vector3(0, 0, 0) + pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 0)+ pos) * BlockScale);
            vertices.Add((new Vector3(0, 0, 1)+ pos) * BlockScale);
            vertices.Add((new Vector3(1, 0, 1)+ pos) * BlockScale);

            AddLastVertSquare();
        }
    }

    public class ChunkData
    {
        public Vector2Int Pos;
        public ChunkRenderer Renderer;
        public BlockType[,,] Blocks;
    }
}