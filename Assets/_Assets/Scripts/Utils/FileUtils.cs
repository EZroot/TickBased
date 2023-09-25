using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;

namespace TickBased.Utils
{
    public static class FileUtils
    {
        public static void InitDirectories(DataSettingsScriptableObject dataSettings)
        {
            CreateDirectoriesIfNoneExist(dataSettings.DataPath);
            CreateDirectoriesIfNoneExist(dataSettings.AssetPath);
            #if UNITY_EDITOR
            CreateLocalProjectDirectory(dataSettings.LocalDevAssetsToCopy);
            CopyLocalFilesFromDirectionTo(dataSettings.LocalDevAssetsToCopy, dataSettings.AssetPath);
            #endif
        }

        static private void CopyLocalFilesFromDirectionTo(string sourcePath, string destinationPath)
        {
            sourcePath = Application.dataPath  + sourcePath;
            destinationPath = Application.persistentDataPath  + destinationPath;
            Logger.Logger.Log($"Copying {sourcePath} -> {destinationPath}","FileUtils");
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(destinationPath, fileName), overwrite: true);
            }
        }
        
        public static void CreateDirectoriesIfNoneExist(string path)
        {
            var newPath = Application.persistentDataPath  + path;
            TickBased.Logger.Logger.Log("Persistent Data Path: " + newPath, "FileUtils");
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
                TickBased.Logger.Logger.Log("Directories <color=green>created</color>: " + newPath, "FileUtils");
            }
            else
            {
                TickBased.Logger.Logger.Log("Directories <color=red>already exist</color>: " + newPath, "FileUtils");
            }
        }
        
        static private void CreateLocalProjectDirectory(string path)
        {
            var newPath = Application.dataPath  + path;
            TickBased.Logger.Logger.Log("Persistent Data Path: " + newPath, "FileUtils");
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
                TickBased.Logger.Logger.Log("Directories <color=green>created</color>: " + newPath, "FileUtils");
            }
            else
            {
                TickBased.Logger.Logger.Log("Directories <color=red>already exist</color>: " + newPath, "FileUtils");
            }
        }

        public static async UniTask SaveFile(string content, string fileName)
        {
            var mngr = ServiceLocator.Get<IServiceGameManager>();
            var dataPath = mngr.GameSettings.DataSettings.DataPath;
            dataPath = dataPath + fileName + ".json";
            var fullPath = Application.persistentDataPath  + dataPath;

            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None,
                       bufferSize: 4096, useAsync: true))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    await writer.WriteAsync(content);
                }
            }

            TickBased.Logger.Logger.Log($"Data saved to file asynchronously: <color=purple>{fullPath}</color>", "FileUtils");
        }

        public static async UniTask<T> LoadFile<T>(string fileName) where T : EntityData
        {
            var mngr = ServiceLocator.Get<IServiceGameManager>();
            var dataPath = mngr.GameSettings.DataSettings.DataPath;
            dataPath = dataPath + fileName + ".json";
            var fullPath = Application.persistentDataPath  + dataPath;

            T loadedData = default(T);
            if (File.Exists(fullPath))
            {
                string jsonData = await File.ReadAllTextAsync(fullPath);
                loadedData = JsonUtility.FromJson<T>(jsonData);
                TickBased.Logger.Logger.Log($"Data loaded from file asynchronously: <color=purple>{fullPath}</color>", "FileUtils");
            }
            else
            {
                TickBased.Logger.Logger.LogError("<color=red>ERROR</color> File not found: " + fullPath, "FileUtils");
            }

            return loadedData;
        }
        
        public static async UniTask<Texture2D> LoadImageToTexture(string fileName)
        {
            var mngr = ServiceLocator.Get<IServiceGameManager>();
            var assetPath = mngr.GameSettings.DataSettings.AssetPath;
            var fullPath = Application.persistentDataPath  + assetPath + fileName;

            if (File.Exists(fullPath))
            {
                byte[] fileData = await File.ReadAllBytesAsync(fullPath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);

                TickBased.Logger.Logger.Log($"Image loaded from file asynchronously: <color=purple>{fullPath}</color>", "FileUtils");
                return texture;
            }
            else
            {
                TickBased.Logger.Logger.LogError("<color=red>ERROR</color> File not found: " + fullPath, "FileUtils");
                return null;
            }
        }
        
        public static async UniTask<Color[]> LoadImageToColorArray(string fileName)
        {
            Texture2D texture = await TickBased.Utils.FileUtils.LoadImageToTexture(fileName);

            if (texture != null)
            {
                Color[] colorArray = texture.GetPixels();
                return colorArray;
            }
            else
            {
                TickBased.Logger.Logger.LogError("<color=red>ERROR</color>: Failed to load image into color array", "DataManager");
                return null;
            }
        }
        
        public static string[] GetAllFilesWithExtension(string directoryPath, string extension)
        {
            directoryPath = Application.persistentDataPath  + directoryPath;
            Logger.Logger.Log($"Trying path: {directoryPath}", "FileUtils");

            if (Directory.Exists(directoryPath))
            {
                string[] filePaths = Directory.GetFiles(directoryPath, $"*{extension}");
                string[] fileNames = new string[filePaths.Length];

                for (int i = 0; i < filePaths.Length; i++)
                {
                    fileNames[i] = Path.GetFileName(filePaths[i]);
                }

                Logger.Logger.Log($"files: {fileNames.Length}", "FileUtils");
                return fileNames;
            }
            else
            {
                TickBased.Logger.Logger.LogError("<color=red>ERROR</color> Directory not found: " + directoryPath, "FileUtils");
                return null;
            }
        }

        
        public static async UniTask<byte[]> LoadFileAsBinary(string fileName)
        {
            var mngr = ServiceLocator.Get<IServiceGameManager>();
            var dataPath = mngr.GameSettings.DataSettings.AssetPath;
            var fullPath = Application.persistentDataPath  + dataPath + fileName;

            if (File.Exists(fullPath))
            {
                byte[] fileData = await File.ReadAllBytesAsync(fullPath);
                TickBased.Logger.Logger.Log($"File loaded asynchronously as binary: <color=purple>{fullPath}</color>", "FileUtils");
                return fileData;
            }
            else
            {
                TickBased.Logger.Logger.LogError("<color=red>ERROR</color> File not found: " + fullPath, "FileUtils");
                return null;
            }
        }

        public static async UniTask SaveTextureToFile(Texture2D texture, string fileName)
        {
            var mngr = ServiceLocator.Get<IServiceGameManager>();
            var assetPath = mngr.GameSettings.DataSettings.AssetPath;
            var fullPath = Application.persistentDataPath  + assetPath + fileName;

            byte[] bytes = texture.EncodeToPNG();
            await File.WriteAllBytesAsync(fullPath, bytes);

            TickBased.Logger.Logger.Log($"Image saved to file asynchronously: <color=purple>{fullPath}</color>", "FileUtils");
        }

        public static async UniTask SaveColorArrayToFile(Color[] colorArray, int width, int height, string fileName)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(colorArray);
            texture.Apply();

            await SaveTextureToFile(texture, fileName);
        }

        public static async UniTask SaveBinaryToFile(byte[] data, string fileName)
        {
            var mngr = ServiceLocator.Get<IServiceGameManager>();
            var dataPath = mngr.GameSettings.DataSettings.AssetPath;
            var fullPath = Application.persistentDataPath  + dataPath + fileName;

            await File.WriteAllBytesAsync(fullPath, data);

            TickBased.Logger.Logger.Log($"File saved asynchronously as binary: <color=purple>{fullPath}</color>", "FileUtils");
        }

    }
}