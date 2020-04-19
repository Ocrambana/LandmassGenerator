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
        public enum DrawMode { NoiseMap, Mesh, FalloffMap };
        public DrawMode drawMode;

        public LandmassGeneration.Script.Data.TerrainData terrainData;
        public NoiseData noiseData;
        public TextureData textureData;

        public Material terrainMaterial;

        [Range(0,6)]
        public int editorPreviewLOD;

        public bool autoUpdate;

        float[,] falloffMap;

        private Queue<MapThreadInfo<MapData>> mapDataInfoQueue = new Queue<MapThreadInfo<MapData>>(); 
        private Queue<MapThreadInfo<MeshData>> meshDataInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        public int mapChunkSize
        {
            get
            {
                if(terrainData.useFlatShading)
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
            textureData.ApplyToMaterial(terrainMaterial);
            textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        }

        private void OnValuesUpdated()
        {
            if(!Application.isPlaying)
            {
                DrawMapInEditor();
            }
        }

        private void OnTextureValuesUpdated()
        {
            if (!Application.isPlaying)
            {
                textureData.ApplyToMaterial(terrainMaterial);
            }
        }

        public void DrawMapInEditor()
        {
            textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

            MapData mapData = GenerateMapData(Vector2.zero);
            MapDisplay display = FindObjectOfType<MapDisplay>();

            if (drawMode == DrawMode.NoiseMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }
            else if(drawMode == DrawMode.Mesh)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshheightCurve, editorPreviewLOD, terrainData.useFlatShading));
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

            if(terrainData.useFalloff)
            {
                if(falloffMap == null || falloffMap.GetLength(0) != mapChunkSize)
                {
                    falloffMap = FalloffGenerator.GenerateFallOffMap(mapChunkSize + 2);
                }

                for(int j = 0; j < mapChunkSize + 2; j++)
                    for(int i = 0; i < mapChunkSize + 2; i++)
                    {
                        noiseMap[i, j] = Mathf.Clamp01(noiseMap[i, j] - falloffMap[i, j]);
                    }
            }

            return new MapData(noiseMap);
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

            if(textureData != null)
            {
                textureData.OnValuesUpdated -= OnTextureValuesUpdated;
                textureData.OnValuesUpdated += OnTextureValuesUpdated;
            }

            falloffMap = FalloffGenerator.GenerateFallOffMap(mapChunkSize + 2);
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

    public struct MapData
    {
        public readonly float[,] heightMap;

        public MapData(float[,] heightMap)
        {
            this.heightMap = heightMap;
        }
    }
}
