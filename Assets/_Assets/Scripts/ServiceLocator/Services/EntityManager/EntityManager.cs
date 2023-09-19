using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EntityManager : MonoBehaviour, IServiceEntityManager
{
    [SerializeField] private EntityTypeToAddressableScriptableObject _entityTypeToAddressable;

    public async UniTask<Entity<T>> LoadEntity<T>(EntityType entityType, GridManager.GridCoordinate gridCoordinate) where T : EntityData
    {
        Entity<T> entity = null;
        // Find the mapping for the given entity type
        string addressableLabel = GetAddressableLabelForEntityType(entityType);
        if (!string.IsNullOrEmpty(addressableLabel))
        {
            var content = await Addressables.LoadAssetAsync<GameObject>(addressableLabel);
            if (content != null)
            {
                var gridManager = ServiceLocator.Get<IServiceGridManager>();
                var cordpos = gridManager.GridToWorld(gridCoordinate);
                var obj = Instantiate(content, cordpos,Quaternion.identity);
                entity = obj.GetComponent<Entity<T>>();
                var networkManager = ServiceLocator.Get<IServiceNetworkManager>();
                networkManager.FishnetManager.ServerManager.Spawn(obj, networkManager.FishnetManager.ClientManager.Connection);
            }
            else
            {
                Debug.LogError("Failed to load entity prefab.");
            }
        }
        else
        {
            Debug.LogError("No mapping found for entity type: " + entityType);
        }

        return entity;
    }

    private string GetAddressableLabelForEntityType(EntityType entityType)
    {
        foreach (var mapping in _entityTypeToAddressable.entityTypeMappings)
        {
            if (mapping.entityType == entityType)
            {
                return mapping.addressableLabel;
            }
        }

        return null; // Return null if no mapping is found
    }
}
