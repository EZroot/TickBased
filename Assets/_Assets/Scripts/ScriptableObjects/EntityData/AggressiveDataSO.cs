using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AggressiveCreatureData", menuName = "EZROOT/EntityData/AggressiveCreatureData")]
public class AggressiveDataSO : DataSO
{
    [SerializeField] private AggressiveEntityData _entityData;
    public AggressiveEntityData EntityData => _entityData;
}