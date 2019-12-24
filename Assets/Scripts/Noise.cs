using System.Collections;
using System;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    internal sealed class Noise
    {
        private static readonly Lazy<Noise> lazy = new Lazy<Noise>(() => new Noise());

        public static Noise Instance { get { return lazy.Value; } }

        private Noise() { }

        int width, height, octaves;

        float   scale, persistance, lacunarity, 
                maxNoise = float.MinValue,
                minNoise = float.MaxValue;
        Vector2[] octavesOffset;

        float[,] noiseMap;

        internal float[,] GenerateMap(int mapWidth, int mapHeight, int seed, float mapScale, int mapOctaves, float mapPersistance, float mapLacunarity, Vector2 offset)
        {
            SetVariables(mapWidth, mapHeight, mapScale, mapOctaves, mapPersistance, mapLacunarity, offset, seed);
            GenerateNoiseMap();
            NormalizeNoiseMap();

            return noiseMap;
        }

        private void SetVariables(int mapWidth, int mapHeight, float mapScale, int mapOctaves, float mapPersistance, float mapLacunarity, Vector2 offset, int seed)
        {
            width = mapWidth;
            height = mapHeight;
            octaves = mapOctaves;
            persistance = mapPersistance;
            lacunarity = mapLacunarity;

            if (mapScale <= 0)
            {
                scale = 0.0001f;
            }
            else
            {
                scale = mapScale;
            }

            GenerateOctaveOffset(offset, seed);
            noiseMap = new float[width, height];
        }

        private void GenerateOctaveOffset(Vector2 offset, int seed)
        {
            System.Random prng = new System.Random(seed);
            octavesOffset = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octavesOffset[i] = new Vector2(offsetX, offsetY);
            }
        }

        private void GenerateNoiseMap()
        {
            for (int j = 0; j < height; j++)
                for (int i = 0; i < width; i++)
                {
                    float noiseHeight = CalculateNoiseHeight(j, i);
                    SetMinMax(noiseHeight);
                    noiseMap[i, j] = noiseHeight;
                }
        }

        private void SetMinMax(float noiseHeight)
        {
            if (noiseHeight > maxNoise)
            {
                maxNoise = noiseHeight;
            }
            else if (noiseHeight < minNoise)
            {
                minNoise = noiseHeight;
            }
        }

        private float CalculateNoiseHeight(int y, int x)
        {
            float
                    halfWidth = width  / 2f,
                    halfHeight = height / 2f,
                    amplitude = 1, 
                    frequency = 1, 
                    noiseHeight = 0;

            for (int o = 0; o < octaves; o++)
            {
                float
                    sampleX = (x - halfWidth) / scale * frequency + octavesOffset[o].x,
                    sampleY = (y - halfHeight) / scale * frequency + octavesOffset[o].y;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            return noiseHeight;
        }

        private void NormalizeNoiseMap()
        {
            for (int j = 0; j < height; j++)
                for (int i = 0; i < width; i++)
                {
                    noiseMap[i, j] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[i, j]);
                }
        }
    }
}
