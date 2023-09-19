using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class GameManager : MonoBehaviour, IServiceGameManager
    {
        [SerializeField] private GameSettings _gameSettings;
        public GameSettings GameSettings => _gameSettings;

        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            LoadServices();
            var uiManager = ServiceLocator.Get<IServiceUIManager>();
            var sceneManager = ServiceLocator.Get<IServiceSceneManager>();
            sceneManager.OnSceneFinishedLoading += uiManager.Initialize;
        }

        void LoadServices()
        {
            ServiceLocator.Register<IServiceGameManager>();
            ServiceLocator.Register<IServiceDataManager>();
            ServiceLocator.Register<IServicePlayerManager>();
            ServiceLocator.Register<IServiceTickManager>();
            ServiceLocator.Register<IServiceNetworkManager>();
            ServiceLocator.Register<IServiceSceneManager>();
            ServiceLocator.Register<IServiceCreatureManager>();
            ServiceLocator.Register<IServiceUIManager>();
            ServiceLocator.Register<IServiceGridManager>();
            ServiceLocator.Register<IServiceLightManager>();
            ServiceLocator.Register<IServiceEntityManager>();
        }
    }
}

[System.Serializable]
public class GameSettings
{
    [SerializeField] DataSettingsScriptableObject _dataSettingsSO;
    public DataSettingsScriptableObject DataSettings => _dataSettingsSO;

}