using System.Collections;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    internal static class Noise
    {
        internal static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];
            float   
                actualScale = scale,
                maxNoiseHeight = float.MinValue,
                minNoiseHeight = float.MaxValue;

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
                            sampleX = i / actualScale * frequency,
                            sampleY = j / actualScale * frequency;

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
