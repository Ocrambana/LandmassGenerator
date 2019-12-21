using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        public int  mapWidth = 10,
                    mapHeight = 10;
        public float noiseScale;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.DrawNoiseMap(noiseMap);
        }
    }
}
