using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectEntityData : CreatureEntityData
{
    public EntityType SpawnOnDeathEntityType;
    public string SpawnOnDeathEntityDataID;
    public bool IsSpawningOnDeath;
    public ObjectEntityData() : base()
    {

    }
}
