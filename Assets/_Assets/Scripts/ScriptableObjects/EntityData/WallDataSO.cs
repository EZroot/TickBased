using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallEntityData", menuName = "EZROOT/EntityData/WallEntityData")]
public class WallDataSO : DataSO
{
    [SerializeField] private WallEntityData _entityData;
    public WallEntityData EntityData => _entityData;
}