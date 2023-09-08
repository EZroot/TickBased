using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableNetworkObjects : MonoBehaviour
{
    [Header("Objects to disable if we do not own this network object")]
    [SerializeField] private GameObject[] _objectsToDisable;
    [SerializeField] private MonoBehaviour[] _scriptsToDisable;

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
