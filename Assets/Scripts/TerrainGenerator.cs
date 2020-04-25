using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.Script
{
    public class TerrainGenerator : MonoBehaviour
    {
        private const float viewerMoveThresholdForChunckUpdate = 25f;
        private const float sqrViewerMoveThresholdForChunckUpdate = viewerMoveThresholdForChunckUpdate * viewerMoveThresholdForChunckUpdate;

        [Range(0, MeshSettings.numSupportedLODs - 1)]
        public int colliderLODIndex;
        public LODInfo[] detailLevels;
        public Transform viewer;
        public Material mapMaterial;

        public MeshSettings meshSettings;
        public HeightMapSettings heightMapSettings;
        public TextureData textureSettings;

        private Vector2 viewerPosition;
        private Vector2 viewerPositionOld;
        float meshWorldSize;
        int chunksVisibleinViewDst;

        private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        private List<TerrainChunk> visibleTerrainChuncks = new List<TerrainChunk>();

        private void Start()
        {
            textureSettings.ApplyToMaterial(mapMaterial);
            textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

            float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
            meshWorldSize = meshSettings.meshWorldSize;
            chunksVisibleinViewDst = Mathf.RoundToInt( maxViewDst / meshWorldSize);
            UpdateVisibleChunks();
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            if(viewerPosition != viewerPositionOld)
            {
                foreach(TerrainChunk chunk in visibleTerrainChuncks)
                {
                    chunk.UpdateCollisionMesh();
                }
            }

            if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunckUpdate)
            {
                viewerPositionOld = viewerPosition;
                UpdateVisibleChunks();
            }
        }

        private void UpdateVisibleChunks()
        {
            HashSet<Vector2> alredyUpdateChunckCoords = new HashSet<Vector2>();
            foreach(TerrainChunk tc in visibleTerrainChuncks.ToArray())
            {
                alredyUpdateChunckCoords.Add(tc.coord);
                tc.UpdateTerrainChunk();
            }

            int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewer.position.z / meshWorldSize);

            for(int yOffset = - chunksVisibleinViewDst; yOffset <= chunksVisibleinViewDst; yOffset++)
                for(int xOffset = - chunksVisibleinViewDst; xOffset <= chunksVisibleinViewDst; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    if(! alredyUpdateChunckCoords.Contains(viewedChunkCoord))
                    {
                        if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                        {
                            terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        }
                        else
                        {
                            TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, mapMaterial, viewer);
                            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                            newChunk.onVisibilityChange += OnTerrainChunckVisibilityChange;
                            newChunk.Load();
                        }
                    }
                }
        }

        private void OnTerrainChunckVisibilityChange(TerrainChunk chunk, bool isVisible)
        {
            if(isVisible)
            {
                visibleTerrainChuncks.Add(chunk);
            }
            else
            {
                visibleTerrainChuncks.Remove(chunk);
            }
        }

    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, MeshSettings.numSupportedLODs - 1)]
        public int lod;
        public float visibleDistanceThreshold;

        public float sqrVisibleDistanceThreshold
        {
            get
            {
                return visibleDistanceThreshold * visibleDistanceThreshold;
            }
        }
    }
}
