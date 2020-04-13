using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocrambana.LandmassGeneration.Script.Data
{
    public class UpdatableData : ScriptableObject
    {
        public System.Action OnValuesUpdated;
        public bool autoUpdate;

        protected virtual void OnValidate()
        {
            if(autoUpdate)
            {
                UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
            }
        }

        public void NotifyOfUpdatedValues()
        {
            UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
            OnValuesUpdated?.Invoke();
        }
    }
}
