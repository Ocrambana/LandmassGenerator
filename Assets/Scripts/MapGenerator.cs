using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColorMap, Mesh };
        public DrawMode drawMode;

        [Range(0,6)]
        public int editorPreviewLOD;
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

        public const int mapChunkSize = 241;

        private Queue<MapThreadInfo<MapData>> mapDataInfoQueue = new Queue<MapThreadInfo<MapData>>(); 
        private Queue<MapThreadInfo<MeshData>> meshDataInfoQueue = new Queue<MapThreadInfo<MeshData>>(); 

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData();
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

                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshheightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
        }

        public void RequestMapData(Action<MapData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MapDataThread(callback);
            };

            new Thread(threadStart).Start();
        }

        private void MapDataThread(Action<MapData> callback)
        {
            MapData mapData = GenerateMapData();

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
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshheightCurve, lod);
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

        private MapData GenerateMapData()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

            Color[] colorMap = GenerateColorMap(noiseMap);

            return new MapData(noiseMap,colorMap);
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
