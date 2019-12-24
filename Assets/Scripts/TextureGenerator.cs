using System.Collections;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public static class TextureGenerator 
    {
        public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colorMap);
            texture.Apply();
            return texture;
        }

        public static Texture2D TextureFromHeightMap(float[,] heightMap)
        {
            int
                width = heightMap.GetLength(0),
                height = heightMap.GetLength(1);

            Texture2D texture = new Texture2D(width, height);

            Color[] colorMap = new Color[width * height];

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    colorMap[j * width + i] = Color.Lerp(Color.black, Color.white, heightMap[i, j]);
                }
            }

            return TextureFromColorMap(colorMap, width, height);
        }
    }
}
