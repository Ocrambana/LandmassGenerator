using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.Script
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap };
        public DrawMode drawMode;

        public LandmassGeneration.Script.Data.TerrainData terrainData;
        public NoiseData noiseData;

        [Range(0,6)]
        public int editorPreviewLOD;

        public TerrainType[] regions;
        static MapGenerator instance;

        public bool autoUpdate;

        float[,] falloffMap;

        private Queue<MapThreadInfo<MapData>> mapDataInfoQueue = new Queue<MapThreadInfo<MapData>>(); 
        private Queue<MapThreadInfo<MeshData>> meshDataInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        public static int mapChunkSize
        {
            get
            {
                if(MapGenerator.instance == null)
                {
                    MapGenerator.instance = FindObjectOfType<MapGenerator>();
                }

                if(MapGenerator.instance.terrainData.useFlatShading)
                {
                    return 95;
                }
                else
                {
                    return 239;
                }
            }
        }

        private void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFallOffMap(mapChunkSize);
        }

        private void OnValuesUpdated()
        {
            if(!Application.isPlaying)
            {
                DrawMapInEditor();
            }
        }

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData(Vector2.zero);
            MapDisplay display = FindObjectOfType<MapDisplay>();

            if (drawMode == DrawMode.NoiseMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }
            else if (drawMode == DrawMode.ColorMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
            else if(drawMode == DrawMode.Mesh)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshheightCurve, editorPreviewLOD, terrainData.useFlatShading), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
            else if(drawMode == DrawMode.FalloffMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(falloffMap));
            }
        }

        public void RequestMapData(Vector2 center, Action<MapData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MapDataThread(center, callback);
            };

            new Thread(threadStart).Start();
        }

        private void MapDataThread(Vector2 center, Action<MapData> callback)
        {
            MapData mapData = GenerateMapData(center);

            lock(mapDataInfoQueue)
            {
                mapDataInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(mapData, lod, callback);
            };

            new Thread(threadStart).Start();
        }

        private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshheightCurve, lod, terrainData.useFlatShading);
            lock(meshDataInfoQueue)
            {
                meshDataInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void Update()
        {
            while(mapDataInfoQueue.Count > 0)
            {
                MapThreadInfo<MapData> threadinfo = mapDataInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }

            while (meshDataInfoQueue.Count > 0)
            {
                MapThreadInfo<MeshData> threadinfo = meshDataInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }

        private MapData GenerateMapData(Vector2 center)
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

            Color[] colorMap = GenerateColorMap(noiseMap);

            return new MapData(noiseMap,colorMap);
        }

        private Color[] GenerateColorMap(float[,] noiseMap)
        {
            Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

            for (int j = 0; j < mapChunkSize; j++)
                for (int i = 0; i < mapChunkSize; i++)
                {
                    if(terrainData.useFalloff)
                    {
                        noiseMap[i,j] = Mathf.Clamp01( noiseMap[i, j] - falloffMap[i, j]); 
                    }

                    float currentHeight = noiseMap[i, j];

                    foreach (TerrainType region in regions)
                    {
                        if (currentHeight >= region.height)
                        {
                            colorMap[j * mapChunkSize + i] = region.color;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            return colorMap;
        }

        private void OnValidate()
        {
            if(noiseData != null)
            {
                noiseData.OnValuesUpdated -= OnValuesUpdated;
                noiseData.OnValuesUpdated += OnValuesUpdated;
            }

            if(terrainData != null)
            {
                terrainData.OnValuesUpdated -= OnValuesUpdated;
                terrainData.OnValuesUpdated += OnValuesUpdated;
            }

            falloffMap = FalloffGenerator.GenerateFallOffMap(mapChunkSize);
        }

        struct MapThreadInfo<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadInfo(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
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

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colorMap;

        public MapData(float[,] heightMap, Color[] colorMap)
        {
            this.heightMap = heightMap;
            this.colorMap = colorMap;
        }
    }
}
