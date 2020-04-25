using System;
using System.Collections.Generic;
using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.Script
{
    public class TerrainChunk
    {
        public event System.Action<TerrainChunk, bool> onVisibilityChange;

        private const float colliderGenerationDistanceThrehold = 5;

        public Vector2 coord;

        private GameObject meshObject;
        private Vector2 sampleCentre;
        private Bounds bounds;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private LODInfo[] detailLevels;
        private LODMesh[] lodMeshes;
        private int colliderLODIndex;

        private HeightMap heightMap;
        private bool heightMapReceived;
        private int previousLODIndex = -1;
        private bool hasSetColldier = false;
        private readonly float maxViewDst;

        private HeightMapSettings heightMapSettings;
        private MeshSettings meshSettings;
        private Transform viewer;

        private Vector2 viewerPosition
        {
            get
            {
                return new Vector2(viewer.position.x, viewer.position.z);
            }
        }

        public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material, Transform viewer)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;
            this.viewer = viewer;

            Vector2 position = coord * meshSettings.meshWorldSize;
            sampleCentre = position / meshSettings.meshScale;
            bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;

            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = new Vector3(position.x, 0, position.y);
            meshObject.transform.SetParent(parent);
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex)
                {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        }

        public void Load()
        {
            ThreadedDataRequester.RequestData(
                () => HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticiesPerLine, meshSettings.numberOfVerticiesPerLine, heightMapSettings, sampleCentre),
                OnMapHeightmapReceived
            );
        }

        private void OnMapHeightmapReceived(object heightMapObject)
        {
            this.heightMap = (HeightMap)heightMapObject;
            heightMapReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!heightMapReceived)
                return;

            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }

            }

            if (wasVisible != visible)
            {
                SetVisible(visible);
                onVisibilityChange?.Invoke(this, visible);
            }
        }

        public void UpdateCollisionMesh()
        {
            if (hasSetColldier)
                return;

            float sqrDistanceViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDistanceViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDistanceViewerToEdge < colliderGenerationDistanceThrehold * colliderGenerationDistanceThrehold)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetColldier = true;
                }
            }
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

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
        {
            hasRequestedMesh = true;
            ThreadedDataRequester.RequestData(
                () => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod),
                OnMeshDataReceived
            );
        }

        void OnMeshDataReceived(object meshDataObject)
        {
            mesh = ((MeshData) meshDataObject).CreateMesh();
            hasMesh = true;

            updateCallback();
        }
    }
}
