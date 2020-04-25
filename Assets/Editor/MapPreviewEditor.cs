using System.Collections;
using UnityEngine;
using UnityEditor;
using Ocrambana.LandmassGeneration.Script;

namespace Ocrambana.LandmassGeneration.EditorExtension
{
    [CustomEditor(typeof(MapPreview))]
    public class MapPreviewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapPreview mapGen = (MapPreview)target;
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
