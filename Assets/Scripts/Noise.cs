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
            Vector2[] octavesOffset = new Vector2[octaves]; 

            for(int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octavesOffset[i] = new Vector2(offsetX, offsetY);
            }

            float   
                actualScale = scale,
                maxNoiseHeight = float.MinValue,
                minNoiseHeight = float.MaxValue;

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
                    float 
                        amplitude = 1,
                        frequency = 1, 
                        noiseHeight = 0;

                    for(int o = 0; o < octaves; o++)
                    {
                        float   
                            sampleX = (i - halfWIdth) / actualScale * frequency + octavesOffset[o].x,
                            sampleY = (j - halfHeight) / actualScale * frequency + octavesOffset[o].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }
                    
                    if(noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if(noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[i, j] = noiseHeight;
                }
            }

            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    noiseMap[i, j] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[i, j]);
                }
            }

             return noiseMap;
        }
    }
}
