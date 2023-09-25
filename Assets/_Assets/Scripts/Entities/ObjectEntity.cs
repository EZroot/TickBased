using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;
using FishNet.Object;
using UnityEngine;

public class ObjectEntity : CreatureEntity<ObjectEntityData>
{
    public override void Start()
    {
        base.Start();

        OnEntityDataChanged += SaveEntityData;
        OnDeath += OnDeath_SpawnResource;
        // -- No reason to check collisions on this? its not sentient --
        // var tickManager = ServiceLocator.Get<IServiceTickManager>();
        // tickManager.OnCommandExecuted +=
        //     OnCommandExecuted_CollisionUpdate; //were doin this to check collision after every movement/attack
        // OnGhostEntityMovement += OnGhostEntityMovement_CollisionUpdate; //updating our ghost so we can execute commands
    }
    void OnDestroy()
    {
        //need to remove from creature manager
        OnEntityDataChanged -= SaveEntityData;
    }

    #region NetworkBehaviour Overrides
    public override void OnStartClient()
    {
        base.OnStartClient();
        //Initialize();
    }
    #endregion
    
    public override void Initialize()
    {
        if (base.IsOwner)
        {
            base.Initialize();
            
            RPCSetEntityDataServer(_entityData);

        }
    }
    
    public override void Initialize(string entityDataKey)
    {
        if (base.IsOwner)
        {
            base.Initialize(entityDataKey);
            
            RPCSetEntityDataServer(_entityData);
        }
    }

    public void OnDeath_SpawnResource()
    {
        var gridMgr = ServiceLocator.Get<IServiceGridManager>();
        gridMgr.SetTileData(_gridCoordinates.X,_gridCoordinates.Y,new GridManager.Tile(GridManager.TileState.Empty,null));
        
        CreatureTransform.gameObject.SetActive(false);
        GhostTransform.gameObject.SetActive(false);
        if (IsOwner && IsServer && _entityData.IsSpawningOnDeath)
        {
            RPCLoadEntityServer(_entityData.SpawnOnDeathEntityType, GridCoordinates, _entityData.SpawnOnDeathEntityDataID);
        }

        _canInteract = false;
        OnDeath -= OnDeath_SpawnResource;
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
            TickBased.Logger.Logger.LogError($"Failed to load entity {entityType}","ObjectEntity");
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
    
    [ServerRpc]
    void RPCSetEntityDataServer(ObjectEntityData data)
    {
        RPCSetEntityDataClient(data);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetEntityDataClient(ObjectEntityData data)
    {
        TickBased.Logger.Logger.Log("Setting Entity Data", "RPCSetEntityDataClient");
        SetEntityData(data);
    }
}
