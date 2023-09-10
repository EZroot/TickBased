using System;
using System.Collections;
using System.Collections.Generic;
using TickBased.Scripts.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InteractionChoice : MonoBehaviour
{
    [SerializeField] private Button _interactionButton;
    [SerializeField] private TMP_Text _buttonText;
    private PlayerEntity _playerReference;
    private ICreatureEntity _referenceInteractionCreature;

    private void OnDestroy()
    {
        UnHighlightSprite();
    }

    public void SetupInteraction(PlayerEntity player, ICreatureEntity creature, string buttonText, Action action)
    {
        _buttonText.text = buttonText;
        _referenceInteractionCreature = creature;
        _playerReference = player;
        _interactionButton.onClick.AddListener(() =>
        {
            action.Invoke();
        });
    }

    public void HighlightSprite()
    {
        if (_referenceInteractionCreature == null)
        {
            Logger.LogError("Failed to Unhighlight sprite!","UI_InteractionChoice");
            return;
        }
        _referenceInteractionCreature.HighlightCreature();
    }

    public void UnHighlightSprite()
    {
        if (_referenceInteractionCreature == null)
        {
            Logger.LogError("Failed to Unhighlight sprite!","UI_InteractionChoice");
            return;
        }
        _referenceInteractionCreature.UnHighlightCreature();
    }
}
