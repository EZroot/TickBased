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
        }

        void LoadServices()
        {
            ServiceLocator.Register<IServiceGameManager>();
            ServiceLocator.Register<IServiceDataManager>();
            ServiceLocator.Register<IServicePlayerManager>();
            ServiceLocator.Register<IServiceTickManager>();
            ServiceLocator.Register<IServiceNetworkManager>();
            ServiceLocator.Register<IServiceNetworkSpawner>();
            ServiceLocator.Register<IServiceSceneManager>();
        }
    }
}

[System.Serializable]
public class GameSettings
{
    [SerializeField] DataSettingsScriptableObject _dataSettingsSO;
    public DataSettingsScriptableObject DataSettings => _dataSettingsSO;

}