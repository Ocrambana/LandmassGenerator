﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class EndlessTerrain : MonoBehaviour
    {
        public const float maxViewDst = 450;
        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        int chunkSize,
            chunksVisibleinViewDst;

        private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        private static MapGenerator mapGenerator;

        private void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunksVisibleinViewDst = Mathf.RoundToInt( maxViewDst / chunkSize);
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            foreach(TerrainChunk tc in terrainChunksVisibleLastUpdate)
            {
                tc.SetVisible(false);
            }

            terrainChunksVisibleLastUpdate.Clear();

            int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewer.position.z / chunkSize);

            for(int yOffset = - chunksVisibleinViewDst; yOffset <= chunksVisibleinViewDst; yOffset++)
                for(int xOffset = - chunksVisibleinViewDst; xOffset <= chunksVisibleinViewDst; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        if(terrainChunkDictionary[viewedChunkCoord].IsVisible())
                        {
                            terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                        }
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                    }
                }

        }

        public class TerrainChunk
        {
            private GameObject meshObject;
            private Vector2 position;
            private Bounds bounds;

            private MeshRenderer meshRenderer;
            private MeshFilter meshFilter;

            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                position = coord * size;
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);
                bounds = new Bounds(position, Vector2.one * size);

                meshObject = new GameObject("Terrein Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.material = material;

                meshFilter = meshObject.AddComponent<MeshFilter>();

                meshObject.transform.position = positionV3;
                meshObject.transform.SetParent(parent);
                SetVisible(false);

                mapGenerator.RequestMapData(OnMapDataReceived);
            }

            private void OnMapDataReceived(MapData mapData)
            {
                mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            }

            private void OnMeshDataReceived(MeshData meshData)
            {
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt( bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;
                SetVisible(visible);
            }

            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

            public bool IsVisible()
            {
                return meshObject.activeSelf;
            }
        }

    }

}
