using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DataLocationEditor : EditorWindow
{
    [MenuItem("EZROOT/Open Data Location")]
    private static void OpenLocation()
    {
        string persistentDataPath = Application.persistentDataPath;
        ShowExplorer(persistentDataPath);
    }

    private static void ShowExplorer(string itemPath)
    {
        itemPath = itemPath.Replace(@"/", @"\");   // Explorer doesn't like forward slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }
}
