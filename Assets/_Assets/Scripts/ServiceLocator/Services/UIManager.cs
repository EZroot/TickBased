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
        private UI_InteractionChoiceManager _uiInteractionChoiceManager;
        public UI_InteractionChoiceManager UIInteractionChoiceManager => _uiInteractionChoiceManager;
        public void Initialize(SceneManager.SceneType sceneType)
        {
            _uiInteractionChoiceManager = GameObject.FindObjectOfType<UI_InteractionChoiceManager>();
        }
    }
}