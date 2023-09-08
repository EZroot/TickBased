using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "EZROOT/EntityData/EntityData")]
public class EntityDataSO : DataSO
{
    [SerializeField] private EntityData _entityData;
    public EntityData EntityData => _entityData;
}
