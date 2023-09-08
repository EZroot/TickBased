using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureEntityData", menuName = "EZROOT/EntityData/CreatureEntityData")]
public class CreatureDataSO : DataSO
{
    [SerializeField] private CreatureEntityData _entityData;
    public CreatureEntityData CreatureEntityData => _entityData;
}
