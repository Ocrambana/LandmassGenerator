﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class MapGenerator : MonoBehaviour
    {
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

        public bool autoUpdate;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.DrawNoiseMap(noiseMap);
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
}
