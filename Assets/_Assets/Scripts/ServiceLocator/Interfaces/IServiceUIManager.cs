namespace FearProj.ServiceLocator
{
    public interface IServiceUIManager : IService
    {
        public UI_InteractionChoiceManager UIInteractionChoiceManager { get; }
        void Initialize(SceneManager.SceneType sceneType);
        void ShowLoadingScreen(string loadingText);
        void SetLoadingScreenText(string text);
        void HideLoadingScreen();
    }
}