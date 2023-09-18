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
        buildPlayerOptions.locationPathName = Path.Combine(buildFolderPath, "Game.exe");
        buildPlayerOptions.target = buildTarget;

        buildPlayerOptions.options = developmentBuild ? BuildOptions.Development : BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
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
}
#endif
