#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildEditorWindow : EditorWindow
{
    private string buildPath;
    private string exeName;
    private BuildTarget buildTarget = BuildTarget.StandaloneWindows;
    private bool developmentBuild = false;
    private bool cleanBuild = false;
    private bool autoOpenFolder = false;

    [MenuItem("Build/Build Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(BuildEditorWindow), false, "Build Project");
    }

    void OnEnable()
    {
        buildPath = Application.dataPath + "/../Build/";
        exeName = "Game.exe";
        autoOpenFolder = true;
    }

    void OnGUI()
    {
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);

        buildPath = EditorGUILayout.TextField("Build Path:", buildPath);
        exeName = EditorGUILayout.TextField("Exe Name:", exeName);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target:", buildTarget);
        developmentBuild = EditorGUILayout.Toggle("Development Build:", developmentBuild);
        autoOpenFolder = EditorGUILayout.Toggle("Auto Open Build Folder:", autoOpenFolder);

        if (GUILayout.Button("Build"))
        {
            PerformBuild();
        }
    }

    void PerformBuild()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        // Generate a timestamp-based folder name
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
    
        // Combine it with the base build path
        string buildFolderName = "Build_" + timestamp;
        string buildFolderPath = Path.Combine(buildPath, buildFolderName);

        // Ensure the directory exists
        Directory.CreateDirectory(buildFolderPath);

        // Set the locationPathName to the new folder path

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = Path.Combine(buildFolderPath, exeName);
        buildPlayerOptions.target = buildTarget;

        buildPlayerOptions.options = developmentBuild ? BuildOptions.Development : BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        CopyFiles(report);

        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            
            if (autoOpenFolder)
            {
                string folderPath = System.IO.Path.GetDirectoryName(buildPath);
                System.Diagnostics.Process.Start(folderPath);
            }
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
        }
    }

    void CopyFiles(BuildReport report)
    {
        string buildPathOutput = report.summary.outputPath;
        string buildPath = Path.GetDirectoryName(buildPathOutput);
        // buildPath += "/" + exeName;
        // buildPath = buildPath.Substring(0, buildPath.Length - 4);
        // buildPath += "_Data";
        
        var dataSettingsPath = "Assets/_Assets/SOSettings/DataSettings.asset";

        DataSettingsScriptableObject dataSettings = AssetDatabase.LoadAssetAtPath<DataSettingsScriptableObject>(dataSettingsPath);
        
        string sourcePath = Application.dataPath + "/.." + dataSettings.LocalDevAssetsToCopy;
        string destinationPath = buildPath + dataSettings.AssetPath;
        Debug.Log($"SOURCE {sourcePath} DEST {destinationPath}");

        // Ensure the destination folder exists
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        // Copy files
        foreach (string file in Directory.GetFiles(sourcePath))
        {
            string fileName = Path.GetFileName(file);
            Debug.Log($"Copying {fileName} ");

            File.Copy(file, Path.Combine(destinationPath, fileName), overwrite: true);
        }

        Debug.Log($"Files copied to {destinationPath}");
    }
}
#endif
