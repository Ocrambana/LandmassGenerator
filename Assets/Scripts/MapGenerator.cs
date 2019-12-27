using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColorMap, Mesh };
        public DrawMode drawMode;

        [Range(0,6)]
        public int levelofDetail;
        public float noiseScale;
        public int octaves;
        [Range(0f,1f)]
        public float persistance = 0.5f;
        public float lacunarity = 2f;

        public int seed;
        public Vector2 offset;

        [Range(1f,1000f)]
        public float meshHeightMultiplier;
        public AnimationCurve meshheightCurve;

        public TerrainType[] regions;

        public bool autoUpdate;

        private Noise noise = null;
        private const int mapChunkSize = 241;

        public void GenerateMap()
        {
            if (noise == null)
            {
                noise = Noise.Instance;
            }

            float[,] noiseMap = noise.GenerateMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

            DrawMap(noiseMap);
        }

        private void DrawMap(float[,] noiseMap)
        {
            MapDisplay display = FindObjectOfType<MapDisplay>();
            Color[] colorMap = GenerateColorMap(noiseMap);

            if (drawMode == DrawMode.NoiseMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            }
            else if (drawMode == DrawMode.ColorMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
            }
            else if(drawMode == DrawMode.Mesh)
            {

                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshheightCurve, levelofDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
            }
        }

        private Color[] GenerateColorMap(float[,] noiseMap)
        {
            Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

            for (int j = 0; j < mapChunkSize; j++)
                for (int i = 0; i < mapChunkSize; i++)
                {
                    float currentHeight = noiseMap[i, j];

                    foreach (TerrainType region in regions)
                    {
                        if (currentHeight <= region.height)
                        {
                            colorMap[j * mapChunkSize + i] = region.color;
                            break;
                        }
                    }
                }

            return colorMap;
        }

        private void OnValidate()
        {
            if(lacunarity < 1)
            {
                lacunarity = 1;
            }

            if(octaves < 0)
            {
                octaves = 0;
            }

        }
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
