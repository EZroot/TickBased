using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InteractionChoiceManager : MonoBehaviour
{
    [SerializeField] private GameObject _UIInteractionChoicePrefab;

    [Header("Obj to spawn prefabs in")]
    [SerializeField] private Transform _interactionChoiceContainer;

    private List<GameObject> _interactionChoiceArray;
    public void SpawnInteractionChoice(PlayerEntity player, ICreatureEntity creatureEntity,  string buttonText, Action interactionAction)
    {
        if (player.UniqueID == creatureEntity.UniqueID)
            return;
        
        _interactionChoiceArray ??= new List<GameObject>();
        var obj = Instantiate(_UIInteractionChoicePrefab, _interactionChoiceContainer);
        var interaction = obj.GetComponent<UI_InteractionChoice>();
        interaction.SetupInteraction(player, creatureEntity,buttonText, interactionAction);
        _interactionChoiceArray.Add(obj);
    }

    public void HideAllInteractionChoices()
    {
        if (_interactionChoiceArray == null || _interactionChoiceArray.Count == 0)
            return;
        
        for (var i = 0; i < _interactionChoiceArray.Count; i++)
        {
            _interactionChoiceArray[i].SetActive(false);
        }

    }
    
    public void ShowAllInteractionChoices()
    {
        if (_interactionChoiceArray == null || _interactionChoiceArray.Count == 0)
            return;
        
        for (var i = 0; i < _interactionChoiceArray.Count; i++)
        {
            _interactionChoiceArray[i].SetActive(true);
        }

    }
    
    public void RemoveAllInteractionChoices()
    {
        if (_interactionChoiceArray == null || _interactionChoiceArray.Count == 0)
            return;
        
        for (var i = 0; i < _interactionChoiceArray.Count; i++)
        {
            Destroy(_interactionChoiceArray[i]);
        }
        
        _interactionChoiceArray.Clear();
    }
}
