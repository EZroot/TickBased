using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectEntityData", menuName = "EZROOT/EntityData/ObjectEntityData")]
public class ObjectEntityDataSO : DataSO
{
    [SerializeField] private ObjectEntityData _entityData;
    public ObjectEntityData EntityData => _entityData;
}
