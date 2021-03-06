﻿using System.Collections;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script
{
    internal static class TextureGenerator 
    {
        internal static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colorMap);
            texture.Apply();

            return texture;
        }

        internal static Texture2D TextureFromHeightMap(HeightMap heightMap)
        {
            int
                width = heightMap.values.GetLength(0),
                height = heightMap.values.GetLength(1);

            Texture2D texture = new Texture2D(width, height);

            Color[] colorMap = new Color[width * height];

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    float saturation = Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[i, j]);
                    colorMap[j * width + i] = Color.Lerp(Color.black, Color.white, saturation);
                }
            }

            return TextureFromColorMap(colorMap, width, height);
        }
    }
}
