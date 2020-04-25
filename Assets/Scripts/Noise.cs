using System.Collections;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script
{
    public enum NormalizeMode { Local, Global };

    internal static class Noise
    {
        internal static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(settings.seed);
            Vector2[] octavesOffset = new Vector2[settings.octaves];

            float 
                maxPossibleHeight = 0,
                amplitude = 1,
                frequency = 1;

            for(int i = 0; i < settings.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
                float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
                octavesOffset[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= settings.persistance;
            }

            float   
                actualScale = settings.scale,
                maxLocalNoiseHeight = float.MinValue,
                minLocalNoiseHeight = float.MaxValue;

            float
                halfWIdth = mapWidth / 2f,
                halfHeight = mapHeight / 2f;

            for(int j = 0; j < mapHeight; j++)
            {
                for(int i = 0; i < mapWidth; i++)
                {
                    amplitude = 1;
                    frequency = 1; 
                    float   noiseHeight = 0;

                    for(int o = 0; o < settings.octaves; o++)
                    {
                        float   
                            sampleX = (i - halfWIdth + octavesOffset[o].x) / actualScale * frequency ,
                            sampleY = (j - halfHeight + octavesOffset[o].y) / actualScale * frequency ;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.persistance;
                        frequency *= settings.lacunarity;
                    }
                    
                    if(noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }

                    if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[i, j] = noiseHeight;

                    if(settings.normalizeMode == NormalizeMode.Global)
                    {
                        float normalizedHeight = (noiseMap[i, j] + 1) / (maxPossibleHeight);
                        noiseMap[i, j] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    }
                }
            }

            if(settings.normalizeMode == NormalizeMode.Local)
            {
                for (int j = 0; j < mapHeight; j++)
                    for (int i = 0; i < mapWidth; i++)
                    {
                        noiseMap[i, j] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[i, j]);
                    }
                   
            }

             return noiseMap;
        }
    }

    [System.Serializable]
    public class NoiseSettings
    {
        public NormalizeMode normalizeMode;

        public float scale = 50;

        public int octaves = 6;
        [Range(0f, 1f)]
        public float persistance = 0.5f;
        public float lacunarity = 2f;

        public Vector2 offset;
        public int seed;

        public void ValidateValues()
        {
            scale = Mathf.Max(scale, 0.01f);
            octaves = Mathf.Max(octaves, 1);
            lacunarity = Mathf.Max(lacunarity, 1);
            persistance = Mathf.Clamp01(persistance);
        }
    }
}
