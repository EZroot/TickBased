using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickBased.Scripts.Commands;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class UIManager : MonoBehaviour, IServiceUIManager
    {
        [SerializeField] private UI_LoadingScreen _uiLoadingScreen;

        private UI_InteractionChoiceManager _uiInteractionChoiceManager;
        public UI_InteractionChoiceManager UIInteractionChoiceManager => _uiInteractionChoiceManager;

        public void Initialize(SceneManager.SceneType sceneType)
        {
            _uiInteractionChoiceManager = GameObject.FindObjectOfType<UI_InteractionChoiceManager>();
        }

        public void SetLoadingScreenText(string text)
        {
            _uiLoadingScreen.SetLoadingScreenText(text);
        }

        public void ShowLoadingScreen(string loadingText)
        {
            _uiLoadingScreen.ShowLoadingScreen(loadingText);
        }

        public void HideLoadingScreen()
        {
            _uiLoadingScreen.HideLoadingScreen();
        }
    }
}