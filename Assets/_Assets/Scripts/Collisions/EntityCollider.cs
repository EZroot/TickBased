using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityCollider : MonoBehaviour
{
    [SerializeField] private GameObject _creatureEntity;
    [ShowInInspector] private ICreatureEntity _creatureEntityInterface;

    void Start()
    {
        _creatureEntityInterface = _creatureEntity.GetComponent<ICreatureEntity>();
    }
    
    public ICreatureEntity GetEntity()
    {
        return _creatureEntityInterface;
    }
}
