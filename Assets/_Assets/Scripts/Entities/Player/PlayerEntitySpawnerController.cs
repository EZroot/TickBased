using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Logger = TickBased.Logger.Logger;
using Random = UnityEngine.Random;

public class PlayerEntitySpawnerController : NetworkBehaviour
{
    private void Update()
    {
        if (IsHost && IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                SpawnLogs();
            }
            
            if (Input.GetKeyDown(KeyCode.O))
            {
                for (var i = -1; i < 1; i++)
                {
                    for (var j = -1; j < 0; j++)
                    {
                        var p = ServiceLocator.Get<IServiceCreatureManager>();
                        var playerPos = p.Player.GridCoordinates;
                        RPCLoadEntityServer(EntityType.Object,
                            new GridManager.GridCoordinate(playerPos.X + 8 + j, playerPos.Y + i), "dev_object2");
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                SpawnDebugHouse();
            }
            
            //dev_aggressive_zombie
            if (Input.GetKeyDown(KeyCode.X))
            {
                SpawnDebugZombie2();

            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                SpawnDebugZombie();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                SpawnAggressive();
            }
            
            if (Input.GetKeyDown(KeyCode.K))
            {
                SpawnTrees();
            }
            
            if (Input.GetKeyDown(KeyCode.J))
            {
                var p = ServiceLocator.Get<IServiceCreatureManager>();
                var playerPos = p.Player.GridCoordinates;
                        RPCLoadEntityServer(EntityType.Object, new GridManager.GridCoordinate(playerPos.X + 3, playerPos.Y), "dev_object2");
            }
        }
    }

    public void SpawnLogs()
    {
        for (var i = -1; i < 1; i++)
        {
            for (var j = -1; j < 0; j++)
            {
                var p = ServiceLocator.Get<IServiceCreatureManager>();
                var playerPos = p.Player.GridCoordinates;
                RPCLoadEntityServer(EntityType.Resource,
                    new GridManager.GridCoordinate(playerPos.X + 8 + j, playerPos.Y + i), "dev_resource_log");
            }
        }
    }
    public void SpawnAggressive()
    {
        for (var i = 0; i < 6; i++)
        {
            var p = ServiceLocator.Get<IServiceCreatureManager>();
            var playerPos = p.Player.GridCoordinates;
            RPCLoadEntityServer(EntityType.AggressiveEntity,
                new GridManager.GridCoordinate(playerPos.X + 4, playerPos.Y + 4+i), "dev_aggressive");
        }
    }
    public void SpawnDebugZombie()
    {
        for (var i = -3; i < 3; i++)
        {
            for (var j = -3; j < 3; j++)
            {
                var p = ServiceLocator.Get<IServiceCreatureManager>();
                var playerPos = p.Player.GridCoordinates;
                RPCLoadEntityServer(EntityType.AggressiveEntity,
                    new GridManager.GridCoordinate(playerPos.X + 8 + j, playerPos.Y + i), "dev_aggressive_zombie");
            }
        }
    }

    public void SpawnTrees()
    {
        var p = ServiceLocator.Get<IServiceCreatureManager>();
        var playerPos = p.Player.GridCoordinates;
        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        HashSet<string> usedCoords = new HashSet<string>();

        float minRadius = 1.0f;
        float maxRadius = 6.0f;
        int numTrees = 50;
        int centerX = playerPos.X + 4;
        int centerY = playerPos.Y;

        for (int n = 0; n < numTrees; n++)
        {
            float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            float radius = UnityEngine.Random.Range(minRadius, maxRadius);
            float noiseX = UnityEngine.Random.Range(-1f, 1f);
            float noiseY = UnityEngine.Random.Range(-1f, 1f);

            int i = centerX + (int)((radius + noiseX) * Mathf.Cos(angle));
            int j = centerY + (int)((radius + noiseY) * Mathf.Sin(angle));

            string coordString = i.ToString() + "," + j.ToString();

            if (!usedCoords.Contains(coordString) && gridManager.GetTile(i,j).State == GridManager.TileState.Empty)
            {
                usedCoords.Add(coordString);
                RPCLoadEntityServer(EntityType.Object, new GridManager.GridCoordinate(i, j), "dev_object2");
            }
            else
            {
                n--;  // Decrement the loop variable to try again
            }
        }
    }
    public void SpawnDebugZombie2()
    {
        for (var i = -3; i < 3; i++)
        {
            for (var j = -3; j < 3; j++)
            {
                var p = ServiceLocator.Get<IServiceCreatureManager>();
                var playerPos = p.Player.GridCoordinates;
                RPCLoadEntityServer(EntityType.AggressiveEntity,
                    new GridManager.GridCoordinate(playerPos.X + 8 + j, playerPos.Y + 8 +i), "dev_aggressive_zombie_old");
            }
        }
    }
    public void SpawnDebugHouse()
    {
        int min = -Random.Range(2,7);
        int max = Random.Range(2,7);
        int doorX = Random.Range(min, max + 1);
        int doorY = Random.Range(min, max + 1);

        // Make sure the door is on the edge
        if (Random.value < 0.5f)
        {
            doorX = (Random.value < 0.5f) ? min : max;
        }
        else
        {
            doorY = (Random.value < 0.5f) ? min : max;
        }
        for (var i = min; i <= max; i++)
        {
            for (var j = min; j <= max; j++)
            {
                // Skip the inner part of the square
                if (i > min && i < max && j > min && j < max)
                {
                    continue;
                }
                // Skip the door position
                if (i == doorX && j == doorY)
                {
                    continue;
                }
                var p = ServiceLocator.Get<IServiceCreatureManager>();
                var playerPos = p.Player.GridCoordinates;
                RPCLoadEntityServer(EntityType.Wall,
                    new GridManager.GridCoordinate(playerPos.X + 8 + j, playerPos.Y + i), "dev_logwall");
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void RPCLoadEntityServer(EntityType entityType, GridManager.GridCoordinate coordinates, string dataKey)
    {
        var dataType = ServiceLocator.Get<IServiceDataManager>().GetEntityDataType(dataKey);
        if (dataType != null)
        {
            MethodInfo method = GetType()
                .GetMethod("LoadAndInitializeEntity", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericMethod = method.MakeGenericMethod(dataType);
            StartCoroutine((IEnumerator)genericMethod.Invoke(this, new object[] { entityType, coordinates, dataKey }));
        }
        else
        {
            Logger.LogError($"Failed to load entity {entityType}","PlayerEntityController");
        }
        //StartCoroutine(LoadAndInitializeEntity<AggressiveEntityData>(entityType, coordinates, dataKey));
    }

    IEnumerator LoadAndInitializeEntity<TDataType>(EntityType entityType, GridManager.GridCoordinate coordinates, string dataKey) where TDataType : EntityData
    {
        var entityManager = ServiceLocator.Get<IServiceEntityManager>();
        var entityTask = entityManager.LoadEntity<TDataType>(entityType, coordinates);
        Entity<TDataType> result = null;
        yield return null;

        yield return UniTask.ToCoroutine(async () =>
        {
            result = await entityTask;
        });
        yield return null;
        result.RPCSetEntityDataKeyServer(dataKey);
    }
}