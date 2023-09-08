using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;

public static class FileUtils
{
    public static void InitDirectories(DataSettingsScriptableObject dataSettings)
    {
        foreach (string path in dataSettings.DataPath)
        {
            CreateDirectoriesIfNoneExist(path);
        }
        foreach (string path in dataSettings.AssetPath)
        {
            CreateDirectoriesIfNoneExist(path);
        }
    }

    static private void CreateDirectoriesIfNoneExist(string path)
    {
        var newPath = Application.persistentDataPath + path;
        Logger.Log("Persistent Data Path: " + newPath, "FileUtils");
        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
            Logger.Log("Directories <color=green>created</color>: " + newPath, "FileUtils");
        }
        else
        {
            Logger.Log("Directories <color=red>already exist</color>: " + newPath, "FileUtils");
        }
    }

    public static async UniTask SaveFile(string content, string fileName)
    {
        var mngr = ServiceLocator.Get<IServiceGameManager>();
        var dataPath = mngr.GameSettings.DataSettings.DataPath[0];
        dataPath = dataPath + fileName + ".json";
        var fullPath = Application.persistentDataPath + dataPath;

        using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        {
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                await writer.WriteAsync(content);
            }
        }

        Logger.Log($"Data saved to file asynchronously: <color=purple>{fullPath}</color>", "FileUtils");
    }

    public static async UniTask<T> LoadFile<T>(string fileName) where T : EntityData
    {
        var mngr = ServiceLocator.Get<IServiceGameManager>();
        var dataPath = mngr.GameSettings.DataSettings.DataPath[0];
        dataPath = dataPath + fileName +".json";
        var fullPath = Application.persistentDataPath + dataPath;

        T loadedData = default(T);
        if (File.Exists(fullPath))
        {
            string jsonData = await File.ReadAllTextAsync(fullPath);
            loadedData = JsonUtility.FromJson<T>(jsonData);
            Logger.Log($"Data loaded from file asynchronously: <color=purple>{fullPath}</color>", "FileUtils");
        }
        else
        {
            Logger.LogError("<color=red>ERROR</color> File not found: " + fullPath, "FileUtils");
        }
        return loadedData;
    }
}