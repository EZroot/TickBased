#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        string buildPathOutput = report.summary.outputPath;
        string buildPath = Path.GetDirectoryName(buildPathOutput);
        var dataSettingsPath = "Assets/_Assets/SOSettings/DataSettings.asset";

        DataSettingsScriptableObject dataSettings = AssetDatabase.LoadAssetAtPath<DataSettingsScriptableObject>(dataSettingsPath);
        
        string sourcePath = Application.dataPath + dataSettings.LocalDevAssetsToCopy;
        string destinationPath = Path.Combine(buildPath, dataSettings.AssetPath);

        // Ensure the destination folder exists
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        // Copy files
        foreach (string file in Directory.GetFiles(sourcePath))
        {
            string fileName = Path.GetFileName(file);
            
            File.Copy(file, Path.Combine(destinationPath, fileName), overwrite: true);
        }

        Debug.Log($"Files copied to {destinationPath}");
    }
}
#endif