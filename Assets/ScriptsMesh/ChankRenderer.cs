using System.Collections.Generic;
using UnityEngine;

namespace GDB.Meshes
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        public const int ChunkWidth = 10;
        public const int ChunkHeight = 128;

        public int[,,] Blocks = null;

        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();

        private void Start()
        {
            Mesh chunkMesh = new Mesh();

            Blocks = TerrainGenerator.GenerateTerrain((int)transform.position.x, (int)transform.position.z);

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

            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            
            chunkMesh.RecalculateNormals();
            chunkMesh.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = chunkMesh;
        }

        private void GenBlock(int x, int y, int z)
        {
            var pos = new Vector3Int(x, y, z);
            
            if(GetBlockInPos(pos) == 0) return;
            
            if(GetBlockInPos(pos + Vector3Int.right) == 0) GenRightSide(pos);
            if(GetBlockInPos(pos + Vector3Int.left) == 0) GenLeftSide(pos);
            if(GetBlockInPos(pos + Vector3Int.forward) == 0) GenFrontSide(pos);
            if(GetBlockInPos(pos + Vector3Int.back) == 0) GenBackSide(pos);
            if(GetBlockInPos(pos + Vector3Int.up) == 0) GenTopSide(pos);
            if(GetBlockInPos(pos + Vector3Int.down) == 0) GenBottomSide(pos);
        }

        private int GetBlockInPos(Vector3Int pos)
        {
            if(pos.x >= 0 && pos.x < ChunkWidth &&
               pos.y >= 0 && pos.y < ChunkHeight &&
               pos.z >= 0 && pos.z < ChunkWidth)
                return Blocks[pos.x, pos.y, pos.z];
            
            return 0;
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
            vertices.Add(new Vector3(0, 0, 0)+ pos);
            vertices.Add(new Vector3(0, 0, 1)+ pos);
            vertices.Add(new Vector3(0, 1, 0)+ pos);
            vertices.Add(new Vector3(0, 1, 1)+ pos);

            AddLastVertSquare();
        }
        
        private void GenFrontSide(Vector3Int pos)
        {
            vertices.Add(new Vector3(0, 0, 1)+ pos);
            vertices.Add(new Vector3(1, 0, 1)+ pos);
            vertices.Add(new Vector3(0, 1, 1)+ pos);
            vertices.Add(new Vector3(1, 1, 1)+ pos);
            

            AddLastVertSquare();
        }
        
        private void GenBackSide(Vector3Int pos)
        {
            vertices.Add(new Vector3(0, 0, 0) + pos);
            vertices.Add(new Vector3(0, 1, 0)+ pos);
            vertices.Add(new Vector3(1, 0, 0)+ pos);
            vertices.Add(new Vector3(1, 1, 0)+ pos);

            AddLastVertSquare();
        }
        
        private void GenTopSide(Vector3Int pos)
        {
            vertices.Add(new Vector3(0, 1, 0) + pos);
            vertices.Add(new Vector3(0, 1, 1)+ pos);
            vertices.Add(new Vector3(1, 1, 0)+ pos);
            vertices.Add(new Vector3(1, 1, 1)+ pos);

            AddLastVertSquare();
        }
        
        private void GenBottomSide(Vector3Int pos)
        {
            vertices.Add(new Vector3(0, 0, 0) + pos);
            vertices.Add(new Vector3(1, 0, 0)+ pos);
            vertices.Add(new Vector3(0, 0, 1)+ pos);
            vertices.Add(new Vector3(1, 0, 1)+ pos);

            AddLastVertSquare();
        }
    }
}