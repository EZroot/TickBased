using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightManager))]
public class LightManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector content
        
        LightManager myScript = (LightManager)target;

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Generate Light Mesh"))
            {
                var gridManager = ServiceLocator.Get<IServiceGridManager>();
                myScript.GenerateLightMesh(gridManager.Grid);
            }
        }
    }
}