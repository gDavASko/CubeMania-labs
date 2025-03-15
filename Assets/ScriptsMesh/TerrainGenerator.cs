using System;
using Unity.Profiling;
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

        private static ProfilerMarker _genMarker = new ProfilerMarker(ProfilerCategory.Loading, "GeneratorTerrain");
        public ChunkData GenerateTerrain(float xOffset, float zOffset)
        {
            _genMarker.Begin();
            
            var result = new BlockType[ChunkRenderer.ChunkWidth * ChunkRenderer.ChunkHeight * ChunkRenderer.ChunkWidth];

            for (int x = 0; x < ChunkRenderer.ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkRenderer.ChunkWidth; z++)
                {
                   float height = GetHeight(x * ChunkRenderer.BlockScale + xOffset, z * ChunkRenderer.BlockScale + zOffset);
                   float grassLayerHeight = 1;
                   float bedrockLayerHeight = 0.5f;
                   
                    for (int y = 0; y < height / ChunkRenderer.BlockScale; y++)
                    {
                        int index = x + y * ChunkRenderer.ChunkWidthSq + z * ChunkRenderer.ChunkWidth;
                        if (height - y * ChunkRenderer.BlockScale < grassLayerHeight)
                        {
                            result[index] = BlockType.Grass;
                        }
                        else if(y * ChunkRenderer.BlockScale < bedrockLayerHeight)
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
            
            _genMarker.End();
            
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