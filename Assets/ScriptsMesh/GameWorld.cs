using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GDB.Meshes
{
    public class GameWorld : MonoBehaviour
    {
        private const int VIEW_RADIUS = 5;
        
        public Dictionary<Vector2Int, ChunkData> ChunkDatas = new Dictionary<Vector2Int, ChunkData>();
        public ChunkRenderer ChunkRendererPrefab;

        private BaseInputActions _input = null;
        [SerializeField] private Camera _camera;
        [SerializeField] private TerrainGenerator generator;
        private Vector2Int curPlayerChunk;
        
        private void Start()
        {
            _input = new BaseInputActions();
            _input.Player.Shoot.performed += OnClickDestroy;
            _input.Player.Create.performed += OnClickCreate;
            _input.Enable();

            if(_camera == null)
                _camera = Camera.main;
            
            StartCoroutine(GenerateChunks(false));
        }

        private IEnumerator GenerateChunks(bool wait)
        {
            for(int x = curPlayerChunk.x - VIEW_RADIUS; x < curPlayerChunk.x + VIEW_RADIUS; x++)
                for (int y = curPlayerChunk.y - VIEW_RADIUS; y < curPlayerChunk.y + VIEW_RADIUS; y++)
                {
                    var chunkPos = new Vector2Int(x, y);
                    if (ChunkDatas.ContainsKey(chunkPos))
                        continue;
                    
                    LoadChunkAt(chunkPos);
                    
                    if(wait)
                        yield return new WaitForSecondsRealtime(0.2f);
                }
        }

        private void LoadChunkAt(Vector2Int chunkPos)
        {
            var xPos = chunkPos.x * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;
            var zPos = chunkPos.y * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;
            var chunkData = new ChunkData();
                    
            chunkData.Pos = chunkPos;
                    
            chunkData.Blocks = generator.GenerateTerrain(xPos, zPos).Blocks;
                    
            ChunkDatas.Add(chunkPos, chunkData);
                    
            var chunk = Instantiate(ChunkRendererPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity, transform);
            chunk.CData = chunkData;
            chunk.World = this;

            chunkData.Renderer = chunk;
        }

        private void Update()
        {
            Vector3 blockPos = _camera.transform.position;
            Vector3Int PlayerWPos = Vector3Int.FloorToInt(blockPos / ChunkRenderer.BlockScale);
            Vector2Int playerChunk = GetChunkContaisBlock(PlayerWPos);

            if (playerChunk != curPlayerChunk)
            {
                curPlayerChunk = playerChunk;
                StartCoroutine(GenerateChunks(true));
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
                Vector3 blockPos = hit.point + hit.normal * ChunkRenderer.BlockScale * 0.5f;
                Vector3Int blockWPos = Vector3Int.FloorToInt(blockPos / ChunkRenderer.BlockScale);
                Vector2Int chunkPos = GetChunkContaisBlock(blockWPos);

                if (ChunkDatas.TryGetValue(chunkPos, out var chunkData))
                {
                    Vector3Int chunkOrig = new Vector3Int(chunkPos.x, 0, chunkPos.y) * ChunkRenderer.ChunkWidth;
                    chunkData.Renderer.SpawnBlock(blockWPos - chunkOrig);
                }
            }
        }
        
        private void OnClickDestroy(InputAction.CallbackContext c)
        {
            var ray = _camera.ViewportPointToRay(Vector3.one * 0.5f);

            if (Physics.Raycast(ray, out var hit))
            {
                Vector3 blockPos = hit.point - hit.normal * ChunkRenderer.BlockScale * 0.5f;
                Vector3Int blockWPos = Vector3Int.FloorToInt(blockPos / ChunkRenderer.BlockScale);
                Vector2Int chunkPos = GetChunkContaisBlock(blockWPos);

                if (ChunkDatas.TryGetValue(chunkPos, out var chunkData))
                {
                    Vector3Int chunkOrig = new Vector3Int(chunkPos.x, 0, chunkPos.y) * ChunkRenderer.ChunkWidth;
                    chunkData.Renderer.DestroyBlock(blockWPos - chunkOrig);
                }
            }
        }

        public Vector2Int GetChunkContaisBlock(Vector3Int blockWPos)
        {
            return new Vector2Int(blockWPos.x / ChunkRenderer.ChunkWidth, blockWPos.z / ChunkRenderer.ChunkWidth);
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
    }
}