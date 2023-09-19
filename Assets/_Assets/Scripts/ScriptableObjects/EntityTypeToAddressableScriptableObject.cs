using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityTypeToAddressableMap", menuName = "ScriptableObjects/EntityTypeToAddressableMap")]
public class EntityTypeToAddressableScriptableObject : ScriptableObject
{
    [Serializable]
    public struct EntityTypeMapping
    {
        public EntityType entityType;
        public string addressableLabel;
    }

    public EntityTypeMapping[] entityTypeMappings;
}