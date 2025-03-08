using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GDB.Meshes
{
    public class GameWorld : MonoBehaviour
    {
        public Dictionary<Vector2Int, ChunkData> ChunkDatas = new Dictionary<Vector2Int, ChunkData>();
        public ChunkRenderer ChunkRendererPrefab;

        private BaseInputActions _input = null;
        [SerializeField] private Camera _camera;
        
        private void Start()
        {
            _input = new BaseInputActions();
            _input.Player.Shoot.performed += OnClickDestroy;
            _input.Player.Create.performed += OnClickCreate;
            _input.Enable();

            if(_camera == null)
                _camera = Camera.main;
            
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

                chunkData.Renderer = chunk;
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
    }
}