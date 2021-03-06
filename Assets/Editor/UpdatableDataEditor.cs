﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.EditorExtension
{
    [CustomEditor(typeof(UpdatableData), true)]
    public class UpdatableDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdatableData data = (UpdatableData) target;

            if(GUILayout.Button ("Update"))
            {
                data.NotifyOfUpdatedValues();
                EditorUtility.SetDirty(target);
            }
        }

    }
}
