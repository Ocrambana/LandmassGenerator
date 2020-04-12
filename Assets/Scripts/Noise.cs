using System.Collections;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script
{
    public enum NormalizeMode { Local, Global };

    internal static class Noise
    {
        internal static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(seed);
            Vector2[] octavesOffset = new Vector2[octaves];

            float 
                maxPossibleHeight = 0,
                amplitude = 1,
                frequency = 1;

            for(int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) - offset.y;
                octavesOffset[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            float   
                actualScale = scale,
                maxLocalNoiseHeight = float.MinValue,
                minLocalNoiseHeight = float.MaxValue;

            float
                halfWIdth = mapWidth / 2f,
                halfHeight = mapHeight / 2f;

            if(scale <= 0)
            {
                actualScale = 0.0001f;
            }

            for(int j = 0; j < mapHeight; j++)
            {
                for(int i = 0; i < mapWidth; i++)
                {
                    amplitude = 1;
                    frequency = 1; 
                    float   noiseHeight = 0;

                    for(int o = 0; o < octaves; o++)
                    {
                        float   
                            sampleX = (i - halfWIdth + octavesOffset[o].x) / actualScale * frequency ,
                            sampleY = (j - halfHeight + octavesOffset[o].y) / actualScale * frequency ;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }
                    
                    if(noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if(noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[i, j] = noiseHeight;
                }
            }

            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    if(normalizeMode == NormalizeMode.Local)
                    {
                        noiseMap[i, j] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[i, j]);
                    }
                    else
                    {
                        float normalizedHeight = (noiseMap[i, j] + 1) / (maxPossibleHeight);
                        noiseMap[i, j] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    }
                }
            }

             return noiseMap;
        }
    }
}
