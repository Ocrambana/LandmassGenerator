using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script.Data
{
    [CreateAssetMenu]
    public class MeshSettings : UpdatableData
    {
        public const int numSupportedLODs = 5;
        public const int numSupportedChunckSizes = 9;
        public const int numSupportedFlatshadedChunckSizes = 3;
        public static readonly int[] supportedChuckSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

        public float meshScale = 1f;
        public bool useFlatShading;

        [Range(0, numSupportedChunckSizes - 1)]
        public int chunkSizeIndex;
        [Range(0, numSupportedFlatshadedChunckSizes - 1)]
        public int flatshadedChunkSizeIndex;

        // number of vericies per line of a mesh rendered at LOD = 0.
        // includes the 2 extra verticies that are excluded from the final mesh, but used for calculating normals
        public int numberOfVerticiesPerLine
        {
            get
            {
                return supportedChuckSizes[(useFlatShading)? flatshadedChunkSizeIndex : chunkSizeIndex] + 5;
            }
        }

        public float meshWorldSize
        {
            get
            {
                return (numberOfVerticiesPerLine - 3) * meshScale;
            }
        }

        #if UNITY_EDITOR

        protected override void OnValidate()
        {
            if(meshScale < 0)
            {
                meshScale = 1f;
            }

            base.OnValidate();
        }

        #endif
    }
}
