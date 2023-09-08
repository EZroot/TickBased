using Cysharp.Threading.Tasks;

namespace FearProj.ServiceLocator
{
    public interface IServiceSceneManager : IService
    {
        event SceneManager.OnSceneChangeDelegate OnSceneFinishedLoading;
        public void LoadSceneCoroutine(SceneManager.SceneType sceneType);

        UniTask LoadSceneAddressableAsync(SceneManager.SceneType sceneType);
    }
}