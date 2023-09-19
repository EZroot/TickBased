using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerEntityController : NetworkBehaviour
{
    private void Update()
    {
        if (IsHost && IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                RPCLoadEntityServer(EntityType.Object, new GridManager.GridCoordinate(3,3),"dev_object2");
            }
        }
    }

    [ServerRpc]
    void RPCLoadEntityServer(EntityType entityType, GridManager.GridCoordinate coordinates, string dataKey)
    {
        StartCoroutine(LoadAndInitializeEntity(entityType, coordinates, dataKey));
        //creature.RPCSendCommandMoveClient(new Vector2(coordinates.X,coordinates.Y), false);
        //RPCLoadEntityClient(entityType, coordinates);
    }

    IEnumerator LoadAndInitializeEntity(EntityType entityType, GridManager.GridCoordinate coordinates, string dataKey)
    {
        var entityManager = ServiceLocator.Get<IServiceEntityManager>();
        var entityTask = entityManager.LoadEntity<ObjectEntityData>(entityType, coordinates);
        Entity<ObjectEntityData> result = null;

        yield return UniTask.ToCoroutine(async () =>
        {
            result = await entityTask;
        });
        Debug.Log($"Spawned result {result!=null}");
        result.RPCSetEntityDataKeyServer(dataKey);
    }
    
    [ObserversRpc]
    void RPCLoadEntityClient(EntityType entityType, GridManager.GridCoordinate coordinates)
    {
    
    }
}