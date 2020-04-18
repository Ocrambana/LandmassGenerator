using System.Linq;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script.Data
{
    [CreateAssetMenu()]
    public class TextureData : UpdatableData
    {
        const int textureSize = 512;
        const TextureFormat textureFormat = TextureFormat.RGB565;

        public Layer[] layers;

        private float savedMinHeight;
        private float savedMaxHeight;


        public void ApplyToMaterial(Material material)
        {
            material.SetInt("layerCount", layers.Length);
            material.SetColorArray("baseColors", layers.Select( x => x.tint).ToArray());
            material.SetFloatArray("baseColorStrength", layers.Select(x => x.tintStrenght).ToArray());
            material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
            material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrenght).ToArray());
            material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());

            Texture2DArray textureArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
            material.SetTexture("baseTexture",textureArray);

            UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
        }

        public void UpdateMeshHeights(Material material, float min, float max)
        {
            savedMinHeight = min;
            savedMaxHeight = max;

            material.SetFloat("minHeight", min);
            material.SetFloat("maxHeight", max);
        }

        private Texture2DArray GenerateTextureArray(Texture2D[] textures)
        {
            Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);

            for(int i = 0; i < textures.Length; i++)
            {
                textureArray.SetPixels(textures[i].GetPixels(), i);
            }

            textureArray.Apply();

            return textureArray;
        }

        [System.Serializable]
        public class Layer
        {
            public Texture2D texture;
            public Color tint;
            [Range(0,1)]
            public float tintStrenght;
            [Range(0,1)]
            public float startHeight;
            [Range(0,1)]
            public float blendStrenght;
            public float textureScale;
        }
    }
}
