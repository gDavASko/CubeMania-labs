using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GDB.Meshes
{
    public class GameWorld : MonoBehaviour
    {
        [SerializeField] private int _viewRadius = 5;
        [SerializeField] private Camera _camera = null;
        [SerializeField] private TerrainGenerator generator = null;
        [SerializeField] private BlocksCommon _config = null;
                
        public Dictionary<Vector2Int, ChunkData> ChunkDatas = new Dictionary<Vector2Int, ChunkData>();
        public ChunkRenderer ChunkRendererPrefab;
        private BaseInputActions _input = null;
        
        private Vector2Int curPlayerChunk;
        
        private ConcurrentQueue<GeneratedMeshData> _meshingsResults = new ConcurrentQueue<GeneratedMeshData>();
        
        private void Start()
        {
            MeshBuilder.SetConfig(_config.BlocksDict, _config.DefaultBlock);
            
            _input = new BaseInputActions();
            _input.Player.Shoot.performed += OnClickDestroy;
            _input.Player.Create.performed += OnClickCreate;
            _input.Enable();

            if(_camera == null)
                _camera = Camera.main;
            
            ChunkRenderer.InitTriangles();
            
            generator.Init();
            StartCoroutine(GenerateChunks(false));
        }

        private IEnumerator GenerateChunks(bool wait)
        {
            int loadRadius = _viewRadius + 1;
            Vector2Int center = curPlayerChunk;
            
            List<ChunkData> loadingChunks = new List<ChunkData>();
            
            for(int x = center.x - loadRadius; x <= center.x + loadRadius; x++)
                for (int y = center.y - loadRadius; y <= center.y + loadRadius; y++)
                {
                    var chunkPos = new Vector2Int(x, y);
                    if (ChunkDatas.ContainsKey(chunkPos))
                        continue;

                    loadingChunks.Add(LoadChunkAt(chunkPos));
                    
                    if(wait)
                        yield return null;
                }

            while (loadingChunks.Any(c => c.State == ChunkDataState.StartedLoading ))
            {
                yield return null;
            }
            
            for(int x = center.x - _viewRadius; x <= center.x + _viewRadius; x++)
                for (int y = center.y - _viewRadius; y <= center.y + _viewRadius; y++)
                {
                    Vector2Int chPos = new Vector2Int(x, y);
                    ChunkData chunkData = ChunkDatas[chPos];
                    
                    if(chunkData.Renderer != null)
                        continue;
                    
                    SpawnChunkRenderer(chunkData);
                        
                    if(wait)
                        yield return null;
                }
        }

        private ChunkData LoadChunkAt(Vector2Int pos)
        {
            var xPos = pos.x * MeshBuilder.ChunkWidth * MeshBuilder.BlockScale;
            var zPos = pos.y * MeshBuilder.ChunkWidth * MeshBuilder.BlockScale;
            
            ChunkData chunkData = new ChunkData();
            chunkData.State = ChunkDataState.StartedLoading;
            chunkData.Pos = pos;
            
            
            ChunkDatas.Add(pos, chunkData);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    chunkData.Blocks = generator.GenerateTerrain(xPos, zPos).Blocks;
                    chunkData.State = ChunkDataState.Loaded;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            });

            return chunkData;
        }
        
        private void SpawnChunkRenderer(ChunkData chunkData)
        { 
            ChunkDatas.TryGetValue(chunkData.Pos + Vector2Int.left, out chunkData.LeftChunk);
            ChunkDatas.TryGetValue(chunkData.Pos + Vector2Int.right, out chunkData.RightChunk);
            ChunkDatas.TryGetValue(chunkData.Pos + Vector2Int.up, out chunkData.FwdChunk);
            ChunkDatas.TryGetValue(chunkData.Pos + Vector2Int.down, out chunkData.BackChunk);

            chunkData.State = ChunkDataState.StartedMeshing;
            
            Task.Factory.StartNew(() =>
            {
                try
                {
                    GeneratedMeshData meshData = MeshBuilder.GenMeshData(chunkData);
                    _meshingsResults.Enqueue(meshData);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            });
        }

        private void Update()
        {
            Vector3 blockPos = _camera.transform.position;
            Vector3Int PlayerWPos = Vector3Int.FloorToInt(blockPos / MeshBuilder.BlockScale);
            Vector2Int playerChunk = GetChunkContaisBlock(PlayerWPos);

            if (playerChunk != curPlayerChunk)
            {
                curPlayerChunk = playerChunk;
                StartCoroutine(GenerateChunks(true));
            }

            if (_meshingsResults.TryDequeue(out var meshData))
            {
                var xPos = meshData.ChunkData.Pos.x * MeshBuilder.ChunkWidth * MeshBuilder.BlockScale;
                var zPos = meshData.ChunkData.Pos.y * MeshBuilder.ChunkWidth * MeshBuilder.BlockScale;
                
                var chunk = Instantiate(ChunkRendererPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity, transform);
                chunk.CData = meshData.ChunkData;
                chunk.World = this;
           
                chunk.SetMesh(meshData);
                meshData.ChunkData.Renderer = chunk;
                
                meshData.ChunkData.State = ChunkDataState.SpawnedInWorld;
            }
        }

        private void OnEnable()
        {
            if(_input != null)
                _input.Enable();
        }

        private void OnDisable()
        {
            if(_input != null)
                _input.Disable();
        }

        private void OnClickCreate(InputAction.CallbackContext c)
        {
            var ray = _camera.ViewportPointToRay(Vector3.one * 0.5f);

            if (Physics.Raycast(ray, out var hit))
            {
                Vector3 blockPos = hit.point + hit.normal * MeshBuilder.BlockScale * 0.5f;
                Vector3Int blockWPos = Vector3Int.FloorToInt(blockPos / MeshBuilder.BlockScale);
                Vector2Int chunkPos = GetChunkContaisBlock(blockWPos);

                if (ChunkDatas.TryGetValue(chunkPos, out var chunkData))
                {
                    Vector3Int chunkOrig = new Vector3Int(chunkPos.x, 0, chunkPos.y) * MeshBuilder.ChunkWidth;
                    chunkData.Renderer.SpawnBlock(blockWPos - chunkOrig);
                }
            }
        }
        
        private void OnClickDestroy(InputAction.CallbackContext c)
        {
            var ray = _camera.ViewportPointToRay(Vector3.one * 0.5f);

            if (Physics.Raycast(ray, out var hit))
            {
                Vector3 blockPos = hit.point - hit.normal * MeshBuilder.BlockScale * 0.5f;
                Vector3Int blockWPos = Vector3Int.FloorToInt(blockPos / MeshBuilder.BlockScale);
                Vector2Int chunkPos = GetChunkContaisBlock(blockWPos);

                if (ChunkDatas.TryGetValue(chunkPos, out var chunkData))
                {
                    Vector3Int chunkOrig = new Vector3Int(chunkPos.x, 0, chunkPos.y) * MeshBuilder.ChunkWidth;
                    chunkData.Renderer.DestroyBlock(blockWPos - chunkOrig);
                }
            }
        }

        public Vector2Int GetChunkContaisBlock(Vector3Int blockWPos)
        {
            Vector2Int pos = new Vector2Int(blockWPos.x / MeshBuilder.ChunkWidth, blockWPos.z / MeshBuilder.ChunkWidth);
            
            if(blockWPos.x < 0) 
                pos.x--;
            if(blockWPos.z < 0) 
                pos.y--;
            
            return pos;
        }

        [ContextMenu("CubeMania/Regenerate")]
        public void Regenerate()
        {
            generator.Init();
            
            foreach (var chunkData in ChunkDatas)
            {
                Destroy(chunkData.Value.Renderer.gameObject);
            }
            ChunkDatas.Clear();

            StartCoroutine(GenerateChunks(false));
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct GeneratedMeshVertex
        {
            public Vector3 pos;
            
            public sbyte normalX;
            public sbyte normalY;
            public sbyte normalZ;
            public sbyte normalW;

            public ushort uvX;
            public ushort uvY;
        }

        public class GeneratedMeshData
        {
            public GeneratedMeshVertex[] Vertices;
            public Bounds Bounds;
            public ChunkData ChunkData;
        }
    }
}