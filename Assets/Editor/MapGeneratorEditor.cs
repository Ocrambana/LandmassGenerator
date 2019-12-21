using System.Collections;
using UnityEngine;
using UnityEditor;

namespace Ocrambana.LandmassGeneration
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapGenerator mapGen = (MapGenerator)target;
            DrawDefaultInspector();

            if(GUILayout.Button("Generate"))
            {
                mapGen.GenerateMap();
            }
        }
    }
}
