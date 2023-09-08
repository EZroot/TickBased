using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCollider<T> : MonoBehaviour where T : CreatureEntityData
{
    //should just make this a IEntityCreature or whatever like the attack command
    [SerializeField] private CreatureEntity<T> _creatureEntity;

    public CreatureEntity<T> GetEntity()
    {
        return _creatureEntity;
    }
}
