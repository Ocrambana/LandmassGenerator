using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script.Data
{
    [CreateAssetMenu()]
    public class NoiseData : UpdatableData
    {
        public NormalizeMode normalizeMode;

        public float noiseScale;

        public int octaves;
        [Range(0f, 1f)]
        public float persistance = 0.5f;
        public float lacunarity = 2f;

        public Vector2 offset;

        public int seed;

        protected override void OnValidate()
        {
            if (lacunarity < 1)
            {
                lacunarity = 1;
            }

            if (octaves < 0)
            {
                octaves = 0;
            }

            base.OnValidate();
        }
    }
}
