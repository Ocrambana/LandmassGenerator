using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script
{
    public class EndlessTerrain : MonoBehaviour
    {
        private const float viewerMoveThresholdForChunckUpdate = 25f;
        private const float sqrViewerMoveThresholdForChunckUpdate = viewerMoveThresholdForChunckUpdate * viewerMoveThresholdForChunckUpdate;
        private const float colliderGenerationDistanceThrehold = 5;

        public int colliderLODIndex;
        public LODInfo[] detailLevels;
        public static float maxViewDst = 600f;
        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        private Vector2 viewerPositionOld;
        int chunkSize,
            chunksVisibleinViewDst;

        private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        private static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        private static MapGenerator mapGenerator;

        private void Start()
        {
            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = mapGenerator.mapChunkSize - 1;
            chunksVisibleinViewDst = Mathf.RoundToInt( maxViewDst / chunkSize);
            UpdateVisibleChunks();
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale ;

            if(viewerPosition != viewerPositionOld)
            {
                foreach(TerrainChunk chunk in terrainChunksVisibleLastUpdate)
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
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, transform, mapMaterial));
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
            private MeshCollider meshCollider;
            private LODInfo[] detailLevels;
            private LODMesh[] lodMeshes;
            private int colliderLODIndex;

            private MapData mapData;
            private bool mapDataReceived;
            private int previousLODIndex = -1;
            private bool hasSetColldier = false;

            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
            {
                this.detailLevels = detailLevels;
                this.colliderLODIndex = colliderLODIndex;

                position = coord * size;
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);
                bounds = new Bounds(position, Vector2.one * size);

                meshObject = new GameObject("Terrain Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.material = material;

                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshCollider = meshObject.AddComponent<MeshCollider>();

                meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
                meshObject.transform.SetParent(parent);
                meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
                SetVisible(false);

                lodMeshes = new LODMesh[detailLevels.Length];
                for(int i=0; i < detailLevels.Length; i++)
                {
                    lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                    lodMeshes[i].updateCallback += UpdateTerrainChunk;
                    if(i == colliderLODIndex)
                    {
                        lodMeshes[i].updateCallback += UpdateCollisionMesh;
                    }
                }

                mapGenerator.RequestMapData(position, OnMapDataReceived);
            }

            private void OnMapDataReceived(MapData mapData)
            {
                this.mapData = mapData;
                mapDataReceived = true;

                UpdateTerrainChunk();
            }

            public void UpdateTerrainChunk()
            {
                if(!mapDataReceived)
                    return;

                float viewerDstFromNearestEdge = Mathf.Sqrt( bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;
                
                if(visible)
                {
                    int lodIndex = 0;

                    for(int i =0; i < detailLevels.Length - 1; i++)
                    {
                        if(viewerDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if(lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes [lodIndex];
                        if(lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if(!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }
                
                SetVisible(visible);
            }

            public void UpdateCollisionMesh()
            {
                if (hasSetColldier)
                    return;

                float sqrDistanceViewerToEdge = bounds.SqrDistance(viewerPosition);

                if(sqrDistanceViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold)
                {
                    if(!lodMeshes[colliderLODIndex].hasRequestedMesh)
                    {
                        lodMeshes[colliderLODIndex].RequestMesh(mapData);
                    }
                }

                if(sqrDistanceViewerToEdge < colliderGenerationDistanceThrehold * colliderGenerationDistanceThrehold)
                {
                    if(lodMeshes[colliderLODIndex].hasMesh)
                    {
                        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        hasSetColldier = true;
                    }
                    else
                    {

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

            public void RequestMesh(MapData mapData)
            {
                hasRequestedMesh = true;
                mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
            }

            void OnMeshDataReceived(MeshData meshData)
            {
                mesh = meshData.CreateMesh();
                hasMesh = true;

                updateCallback();
            }
        }

        [System.Serializable]
        public struct LODInfo
        {
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


}
