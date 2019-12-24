using System.Collections;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    internal static class Noise
    {
        internal static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(seed);
            Vector2[] octavesOffset = GenerateOctaveOffset(octaves, offset, prng);

            float
                actualScale = scale,
                maxNoiseHeight = float.MinValue,
                minNoiseHeight = float.MaxValue;

            float
                halfWIdth = mapWidth / 2f,
                halfHeight = mapHeight / 2f;

            if (scale <= 0)
            {
                actualScale = 0.0001f;
            }

            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    float noiseHeight = CalculateNoiseHeight(octaves, persistance, lacunarity, octavesOffset, actualScale, halfWIdth, halfHeight, j, i);

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[i, j] = noiseHeight;
                }
            }


            noiseMap = NormalizeNoiseMap(maxNoiseHeight, minNoiseHeight, noiseMap);

            return noiseMap;
        }

        private static float CalculateNoiseHeight(int octaves, float persistance, float lacunarity, Vector2[] octavesOffset, float actualScale, float halfWIdth, float halfHeight, int y, int x)
        {
            float
                    amplitude = 1, 
                    frequency = 1, 
                    noiseHeight = 0;

            for (int o = 0; o < octaves; o++)
            {
                float
                    sampleX = (x - halfWIdth) / actualScale * frequency + octavesOffset[o].x,
                    sampleY = (y - halfHeight) / actualScale * frequency + octavesOffset[o].y;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            return noiseHeight;
        }

        private static Vector2[] GenerateOctaveOffset(int octaves, Vector2 offset, System.Random prng)
        {
            Vector2[] octavesOffset = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octavesOffset[i] = new Vector2(offsetX, offsetY);
            }

            return octavesOffset;
        }

        private static float[,] NormalizeNoiseMap(float max, float min, float[,] noiseMap)
        {
            int width = noiseMap.GetLength(0),
                height = noiseMap.GetLength(1);

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    noiseMap[i, j] = Mathf.InverseLerp(min, max, noiseMap[i, j]);
                }
            }

            return noiseMap;
        }
    }
}
