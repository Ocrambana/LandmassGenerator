using System.Collections;
using UnityEngine;
using UnityEditor;
using Ocrambana.LandmassGeneration.Script;

namespace Ocrambana.LandmassGeneration.EditorExtension
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapGenerator mapGen = (MapGenerator)target;
            if(DrawDefaultInspector())
            {
                if(mapGen.autoUpdate)
                {
                    mapGen.DrawMapInEditor();
                }
            }

            if(GUILayout.Button("Generate"))
            {
                mapGen.DrawMapInEditor();
            }
        }
    }
}
