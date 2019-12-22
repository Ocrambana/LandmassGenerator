using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    public class MapDisplay : MonoBehaviour
    {
        public Renderer textureRenderer;
        
        public void DrawNoiseMap(float[,] noiseMap)
        {
            int 
                width = noiseMap.GetLength(0),
                height = noiseMap.GetLength(1);

            Texture2D texture = new Texture2D(width, height);

            Color[] colorMap = new Color[width * height];

            for(int j = 0; j < height; j++)
            {
                for(int i = 0; i < width; i++)
                {
                    colorMap[j * width + i] = Color.Lerp(Color.black, Color.white, noiseMap[i, j]);
                }
            }

            texture.SetPixels(colorMap);
            texture.Apply();

            textureRenderer.sharedMaterial.mainTexture = texture;
            textureRenderer.transform.localScale = new Vector3(width, 1f, height);
        }
    }
}
