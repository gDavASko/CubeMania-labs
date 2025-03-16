using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GDB.Meshes
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        [SerializeField] private BlocksCommon _config = null;

        public ChunkData CData = null;
        public GameWorld World = null;

        private List<int> triangles = new List<int>();

        private Mesh chunkMesh;

        private static int[] _triangles;

        private void Awake()
        {
            chunkMesh = new Mesh();

            GetComponent<MeshFilter>().mesh = chunkMesh;
        }

        public void SpawnBlock(Vector3Int pos)
        {
            int index = pos.x + pos.y * MeshBuilder.ChunkWidthSq + pos.z * MeshBuilder.ChunkWidth;
            CData.Blocks[index] = BlockType.Dirt;
            RegenerateMesh();
        }

        public void DestroyBlock(Vector3Int pos)
        {
            int index = pos.x + pos.y * MeshBuilder.ChunkWidthSq + pos.z * MeshBuilder.ChunkWidth;
            CData.Blocks[index] = BlockType.Air;
            RegenerateMesh();
        }

        private void RegenerateMesh()
        {
            SetMesh(MeshBuilder.GenMeshData(CData));
        }

        public static void InitTriangles()
        {
            _triangles = new int[65536 * 6 / 4];

            int vertNum = 4;
            for (int i = 0; i < _triangles.Length; i += 6)
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

        public void SetMesh(GameWorld.GeneratedMeshData meshData)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.UNorm16, 2)
            };

            chunkMesh.SetVertexBufferParams(meshData.Vertices.Length, layout);
            chunkMesh.SetVertexBufferData(meshData.Vertices, 0, 0, meshData.Vertices.Length,
                0, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds);

            int trianglesCount = meshData.Vertices.Length / 4 * 6;
            chunkMesh.SetIndexBufferParams(trianglesCount, IndexFormat.UInt32);
            chunkMesh.SetIndexBufferData(_triangles, 0, 0, trianglesCount,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds );

            chunkMesh.subMeshCount = 1;
            chunkMesh.SetSubMesh(0, new SubMeshDescriptor(0, trianglesCount));

            chunkMesh.bounds = meshData.Bounds;

            GetComponent<MeshCollider>().sharedMesh = chunkMesh;
        }
    }
}