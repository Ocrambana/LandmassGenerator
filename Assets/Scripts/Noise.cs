using System.Collections;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    internal static class Noise
    {
        internal static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];
            float actualScale = scale;

            if(scale <= 0)
            {
                actualScale = 0.0001f;
            }

            for(int j = 0; j < mapHeight; j++)
            {
                for(int i = 0; i < mapWidth; i++)
                {
                    float   sampleX = i / actualScale,
                            sampleY = j / actualScale;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseMap[i, j] = perlinValue;
                    
                }
            }

            return noiseMap;
        }
    }
}
