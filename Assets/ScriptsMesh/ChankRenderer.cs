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

            //CData = TerrainGenerator.GenerateTerrain((int)transform.position.x, (int)transform.position.z);

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
            CData.Blocks[pos.x, pos.y, pos.z] = BlockType.Stone;
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

        private void AddUVs(BlockType type, Vector2Int normal)
        {
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
        }
        
        /*private void AddUVs(BlockType blockType, Vector2Int normal)
        {
            Vector2 uv;
            if (blockType == BlockType.Grass)
            {
                uv = normal == Vector2Int.up ? new Vector2(32f / 256, 240f / 256) :
                    normal == Vector2Int.down ? new Vector2(32f / 256, 240f / 256) :
                    new Vector2(32f / 256, 240f / 256);
            }
            else if (blockType == BlockType.Stone)
            {
                uv = new Vector2(16f / 256, 240f / 256);
            }
            else if (blockType == BlockType.Wood)
            {
                uv = new Vector2(64f / 256, 240f / 256);
            }
            else
            {
                uv = new Vector2(160f / 256, 224f / 256);
            }

            for (int i = 0; i < 4; i++)
            {
                uvs.Add(uv);
            }
        }*/

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