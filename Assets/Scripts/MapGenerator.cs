using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColorMap, Mesh };
        public DrawMode drawMode;

        public int
            mapWidth = 10,
            mapHeight = 10;
        public float noiseScale;

        public int octaves;
        [Range(0f,1f)]
        public float persistance = 0.5f;
        public float lacunarity = 2f;

        public int seed;
        public Vector2 offset;

        public TerrainType[] regions;

        public bool autoUpdate;

        private Noise noise = null;

        public void GenerateMap()
        {
            if (noise == null)
            {
                noise = Noise.Instance;
            }

            float[,] noiseMap = noise.GenerateMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

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
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
            }
            else if(drawMode == DrawMode.Mesh)
            {

                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
            }
        }

        private Color[] GenerateColorMap(float[,] noiseMap)
        {
            Color[] colorMap = new Color[mapWidth * mapHeight];

            for (int j = 0; j < mapHeight; j++)
                for (int i = 0; i < mapWidth; i++)
                {
                    float currentHeight = noiseMap[i, j];

                    foreach (TerrainType region in regions)
                    {
                        if (currentHeight <= region.height)
                        {
                            colorMap[j * mapWidth + i] = region.color;
                            break;
                        }
                    }
                }

            return colorMap;
        }

        private void OnValidate()
        {
            if(mapWidth < 1)
            {
                mapWidth = 1;
            }

            if(mapHeight < 1)
            {
                mapHeight = 1;
            }

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
