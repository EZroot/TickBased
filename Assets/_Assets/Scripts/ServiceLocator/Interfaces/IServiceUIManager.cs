using System;
using TickBased.Scripts.Commands;

namespace FearProj.ServiceLocator
{
    public interface IServiceUIManager : IService
    {
        public UI_InteractionChoiceManager UIInteractionChoiceManager { get; }
        void Initialize(SceneManager.SceneType sceneType);
    }
}