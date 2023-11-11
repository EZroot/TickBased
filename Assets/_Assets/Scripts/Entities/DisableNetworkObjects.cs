using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableNetworkObjects : MonoBehaviour
{
    [Header("Objects to disable if we do not own this network object")]
    [SerializeField] private GameObject[] _objectsToDisable;
    [SerializeField] private MonoBehaviour[] _scriptsToDisable;
    [Header("These are disabled if you are NOT the host")]
    [SerializeField] private GameObject[] _notHostGameObjectsToDisable;
    [SerializeField] private MonoBehaviour[] _notHostScriptsToDisable;

    public void DisableScriptsIfNotHost(bool isEnabled)
    {
        if (_notHostScriptsToDisable.Length > 0)
        {
            foreach (var scr in _notHostScriptsToDisable)
            {
                scr.enabled = isEnabled;
            }
        }

        if (_notHostGameObjectsToDisable.Length > 0)
        {
            foreach (var ob in _notHostGameObjectsToDisable)
            {
                ob.SetActive(isEnabled);
            }
        }
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
