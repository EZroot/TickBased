using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ResourceEntityData", menuName = "EZROOT/EntityData/ResourceEntityData")]
public class ResourceDataSO : DataSO
{
    [SerializeField] private ResourceEntityData _entityData;
    public ResourceEntityData EntityData => _entityData;
}