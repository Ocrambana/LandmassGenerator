using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class EndlessTerrain : MonoBehaviour
    {
        private const float viewerMoveThresholdForChunckUpdate = 25f;
        const float sqrViewerMoveThresholdForChunckUpdate = viewerMoveThresholdForChunckUpdate * viewerMoveThresholdForChunckUpdate;
        public LODInfo[] detailLevels;
        public static float maxViewDst = 450;
        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        private Vector2 viewerPositionOld;
        int chunkSize,
            chunksVisibleinViewDst;

        private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        private static MapGenerator mapGenerator;

        private void Start()
        {
            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunksVisibleinViewDst = Mathf.RoundToInt( maxViewDst / chunkSize);
            UpdateVisibleChunks();
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

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
                        if(terrainChunkDictionary[viewedChunkCoord].IsVisible())
                        {
                            terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                        }
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
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
            private LODInfo[] detailLevels;
            private LODMesh[] LODMeshes;
            private MapData mapData;
            private bool mapDataReceived;
            private int previousLODIndex = -1;       

            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
            {
                this.detailLevels = detailLevels;
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

                LODMeshes = new LODMesh[detailLevels.Length];
                for(int i=0; i < detailLevels.Length; i++)
                {
                    LODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                }

                mapGenerator.RequestMapData(position, OnMapDataReceived);
            }

            private void OnMapDataReceived(MapData mapData)
            {
                this.mapData = mapData;
                mapDataReceived = true;

                Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
                meshRenderer.material.mainTexture = texture;

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
                        if(viewerDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }

                    if(lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = LODMeshes [lodIndex];
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
                }
                
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

        class LODMesh 
        {
            public Mesh mesh;
            public bool hasRequestedMesh;
            public bool hasMesh;
            private int lod;
            System.Action updateCallback;

            public LODMesh(int lod, System.Action updateCallback)
            {
                this.lod = lod;
                this.updateCallback = updateCallback;
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
        }

    }


}
