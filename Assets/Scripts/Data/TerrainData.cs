using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script.Data
{
    [CreateAssetMenu]
    public class TerrainData : UpdatableData
    {
        public float uniformScale = 1f;

        public bool useFlatShading;
        public bool useFalloff;

        [Range(1f, 100f)]
        public float meshHeightMultiplier;
        public AnimationCurve meshheightCurve;

        public float minHeight
        {
            get
            {
                return uniformScale * meshHeightMultiplier * meshheightCurve.Evaluate(0);
            }
        }

        public float maxHeight
        {
            get
            {
                return uniformScale * meshHeightMultiplier * meshheightCurve.Evaluate(1);
            }
        }

        protected override void OnValidate()
        {
            if(uniformScale < 0)
            {
                uniformScale = 1f;
            }

            base.OnValidate();
        }
    }
}
