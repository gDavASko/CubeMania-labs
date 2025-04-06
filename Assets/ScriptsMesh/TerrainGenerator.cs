using System;
using UnityEngine;

namespace GDB.Meshes
{
    [CreateAssetMenu(fileName = "TerrainGenerator", menuName = "GDB/TerrainGenerator")]
    public class TerrainGenerator: ScriptableObject
    {
        [SerializeField] private float _baseHeight = 8;
        [SerializeField] private NoiseOctaveSettings[] _noiseSettings;
        [SerializeField] private NoiseOctaveSettings _domainWarpSettings;
        
        private FastNoiseLite warpNoise;
        private FastNoiseLite[] _octaveNoises;

        public void Init()
        {
            _octaveNoises = new FastNoiseLite[_noiseSettings.Length];

            for (int i = 0; i < _noiseSettings.Length; i++)
            {
                _octaveNoises[i] = new FastNoiseLite();
                _octaveNoises[i].SetNoiseType(_noiseSettings[i].NoiseType);
                _octaveNoises[i].SetFrequency(_noiseSettings[i].Frequence);
            }
            
            warpNoise = new FastNoiseLite();
            warpNoise.SetNoiseType(_domainWarpSettings.NoiseType);
            warpNoise.SetFrequency(_domainWarpSettings.Frequence);
            warpNoise.SetDomainWarpAmp(_domainWarpSettings.Amplitude);
        }

        public ChunkData GenerateTerrain(float xOffset, float zOffset)
        {
            if (_octaveNoises == null || warpNoise == null)
            {
                Debug.LogError("TerrainGenerator not initialized! Call Init() first.");
                Init(); // Auto-initialize if needed
            }
            
            var result = new BlockType[MeshBuilder.ChunkWidth * MeshBuilder.ChunkHeight * MeshBuilder.ChunkWidth];

            for (int x = 0; x < MeshBuilder.ChunkWidth; x++)
            {
                for (int z = 0; z < MeshBuilder.ChunkWidth; z++)
                {
                   float height = GetHeight(x * MeshBuilder.BlockScale + xOffset, z * MeshBuilder.BlockScale + zOffset);
                   float grassLayerHeight = 1;
                   float bedrockLayerHeight = 0.5f;
                   
                    for (int y = 0; y < height / MeshBuilder.BlockScale; y++)
                    {
                        int index = x + y * MeshBuilder.ChunkWidthSq + z * MeshBuilder.ChunkWidth;
                        if (height - y * MeshBuilder.BlockScale < grassLayerHeight)
                        {
                            result[index] = BlockType.Grass;
                        }
                        else if(y * MeshBuilder.BlockScale < bedrockLayerHeight)
                        {
                            result[index] = BlockType.Bedrock;
                        }
                        else
                        {
                            result[index] = BlockType.Dirt;
                        }
                    }
                }
            }
            
            return new ChunkData() { Blocks = result };
        }

        private float GetHeight(float x, float y)
        {
            warpNoise.DomainWarp(ref x, ref y);
            
            float res = _baseHeight;

            for (int i = 0; i < _noiseSettings.Length; i++)
            {
                float noise = _octaveNoises[i].GetNoise(x, y) * _noiseSettings[i].Amplitude / 2f;
                res += noise;
            }
            
            return res;
        }

        [Serializable]
        private class NoiseOctaveSettings
        {
            public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.Perlin;
            public float Frequence = 0.2f;
            public float Amplitude = 1f;
        }
    }
}