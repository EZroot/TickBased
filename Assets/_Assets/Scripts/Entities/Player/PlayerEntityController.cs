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
                for(int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        RPCLoadEntityServer(EntityType.Object, new GridManager.GridCoordinate(i,j),"dev_object2");
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                RPCLoadEntityServer(EntityType.Object, new GridManager.GridCoordinate(0,0),"dev_object2");
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                RPCLoadEntityServer(EntityType.Object, new GridManager.GridCoordinate(3,0),"dev_object2");
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
        yield return null;

        yield return UniTask.ToCoroutine(async () =>
        {
            result = await entityTask;
        });
        yield return null;
        result.RPCSetEntityDataKeyServer(dataKey);
    }
}