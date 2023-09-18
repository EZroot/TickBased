using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace FearProj.ServiceLocator
{
    public class SceneManager : MonoBehaviour, IServiceSceneManager
    {

        public enum SceneType
        {
            BootstrapScene,
            LobbyScene,
            GameScene
        }

        public delegate void OnSceneChangeDelegate(SceneType sceneType);
        public event OnSceneChangeDelegate OnSceneFinishedLoading;

        [SerializeField] private SceneSettingsScriptableObject _sceneSettingsSO;
        [Header("Tip: This scene should also be loaded before hitting play.")]
        [SerializeField] private SceneType _firstScene;
        [SerializeField] private bool _loadFirstSceneOnPlay = false;

        SceneInstance _currentActiveSceneInstance;


        async void Start()
        {
            await Initialize();
        }

        async UniTask Initialize()
        {
            if (_loadFirstSceneOnPlay)
            {
                await LoadSceneAddressableAsync(_firstScene);
            }
        }

        public void LoadSceneCoroutine(SceneType sceneType)
        {
            StartCoroutine(LoadMainScene(sceneType));
        }
        IEnumerator LoadMainScene(SceneManager.SceneType scene)
        {
            yield return LoadSceneAddressableAsync(scene);
        }
        
        public async UniTask LoadSceneAddressableAsync(SceneType sceneType)
        {
            string sceneName = sceneType switch
            {
                SceneType.BootstrapScene => _sceneSettingsSO.BootstrapSceneName,
                SceneType.LobbyScene => _sceneSettingsSO.LobbySceneName,
                SceneType.GameScene => _sceneSettingsSO.GameSceneName,
                _ => "<color=red>SCENE TYPE DOESNT EXIST</color>"
            };

            if (_currentActiveSceneInstance.Scene.isLoaded)
            {
                var unloadResult = await Addressables.UnloadSceneAsync(_currentActiveSceneInstance, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            }
            _currentActiveSceneInstance = await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if(OnSceneFinishedLoading != null)
            {
                TickBased.Logger.Logger.Log($"On scene finished loading {sceneName}");
                OnSceneFinishedLoading(sceneType);
            }
        }
    }
}