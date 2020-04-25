using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.Script
{
    public class MapPreview : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, Mesh, FalloffMap };
        public DrawMode drawMode;

        public MeshSettings meshSettings;
        public HeightMapSettings heightMapSettings;
        public TextureData textureData;

        public Material terrainMaterial;

        [Range(0, MeshSettings.numSupportedLODs - 1)]
        public int editorPreviewLOD;

        public Renderer textureRenderer;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public bool autoUpdate;

        float[,] falloffMap;

        private void Start()
        {
            if(Application.isPlaying)
            {
                gameObject.SetActive(false);
            }
        }

        public void DrawMapInEditor()
        {
            textureData.ApplyToMaterial(terrainMaterial);
            textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

            HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticiesPerLine, meshSettings.numberOfVerticiesPerLine, heightMapSettings, Vector2.zero);

            if (drawMode == DrawMode.NoiseMap)
            {
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
            }
            else if (drawMode == DrawMode.Mesh)
            {
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
            }
            else if (drawMode == DrawMode.FalloffMap)
            {
                DrawTexture(TextureGenerator.TextureFromHeightMap( new HeightMap (falloffMap, 0, 1)));
            }
        }

        public void DrawTexture(Texture2D texture)
        {
            textureRenderer.sharedMaterial.mainTexture = texture;
            textureRenderer.transform.localScale = new Vector3(texture.width, 1f, texture.height) / 10f;

            textureRenderer.gameObject.SetActive(true);
            meshFilter.gameObject.SetActive(false);
        }

        public void DrawMesh(MeshData meshData)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();

            textureRenderer.gameObject.SetActive(false);
            meshFilter.gameObject.SetActive(true);
        }

        private void OnValuesUpdated()
        {
            if (!Application.isPlaying)
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

        private void OnValidate()
        {
            if (heightMapSettings != null)
            {
                heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
                heightMapSettings.OnValuesUpdated += OnValuesUpdated;
            }

            if (meshSettings != null)
            {
                meshSettings.OnValuesUpdated -= OnValuesUpdated;
                meshSettings.OnValuesUpdated += OnValuesUpdated;
            }

            if (textureData != null)
            {
                textureData.OnValuesUpdated -= OnTextureValuesUpdated;
                textureData.OnValuesUpdated += OnTextureValuesUpdated;
            }

            falloffMap = FalloffGenerator.GenerateFallOffMap(meshSettings.numberOfVerticiesPerLine);
        }
    }
}
