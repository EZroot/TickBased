using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector content
        
        GridManager myScript = (GridManager)target;

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Generate Mesh"))
            {
                myScript.GenerateMesh();
            }
        }
    }
}
