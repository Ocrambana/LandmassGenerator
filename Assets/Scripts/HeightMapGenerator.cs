﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.Script
{
    public static class HeightMapGenerator
    {
        public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
        {
            float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

            AnimationCurve heightCurve_threadSafe = new AnimationCurve(settings.heightCurve.keys);

            float   minValue = float.MaxValue,
                    maxValue = float.MinValue;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    values[i, j] *= heightCurve_threadSafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                    if(values[i,j] > maxValue)
                    {
                        maxValue = values[i, j];
                    }
                    if(values[i,j] > minValue)
                    {
                        minValue = values[i, j];
                    }
                }
            }

            return new HeightMap(values, minValue, maxValue);
        }
    }

    public struct HeightMap
    {
        public readonly float[,] values;
        public readonly float minValue;
        public readonly float maxValue;

        public HeightMap(float[,] heightMap, float minVal, float maxValue)
        {
            this.values = heightMap;
            this.minValue = minVal;
            this.maxValue = maxValue;
        }
    }
}
