using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using FishNet.Object;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Objects to disable if we do not own this network object")]
    [SerializeField] private GameObject[] _objectsToDisable;
    [SerializeField] private MonoBehaviour[] _scriptsToDisable;
    
    public void MoveEntity(Vector2 direction, float speed)
    {
            TickBased.Logger.Logger.Log("Moving entity", "PlayerController");
            transform.Translate(direction * speed * Time.deltaTime);
    }

    public void EnableOwnerObjects(bool isEnabled)
    {
        if (_objectsToDisable.Length > 0)
        {
            foreach (var obj in _objectsToDisable)
            {
                obj.SetActive(isEnabled);
            }
        }

        if (_scriptsToDisable.Length > 0)
        {
            foreach (var comp in _scriptsToDisable)
            {
                comp.enabled = isEnabled;
            }
        }
    }
}
