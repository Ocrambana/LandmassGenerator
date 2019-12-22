using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        public int  
            mapWidth = 10,
            mapHeight = 10,
            octaves;
        public float   
            noiseScale,
            persistance = 0.5f,
            lacunarity = 2f;
        public bool autoUpdate;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, octaves, persistance, lacunarity);

            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.DrawNoiseMap(noiseMap);
        }
    }
}
