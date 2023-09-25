using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;
using FishNet.Object;
using TickBased.Scripts.Commands;
using UnityEngine;

public class ResourceEntity : CreatureEntity<ResourceEntityData>, IInventoryItem
{
    private bool _canPickup = true;
    public bool CanPickup => _canPickup;

    public ItemType Item => _entityData.Resource;
    
    public override void Start()
    {
        base.Start();

        OnEntityDataChanged += SaveEntityData;
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
    
    public override void Initialize(string entityDataKey)
    {
        if (base.IsOwner)
        {
            base.Initialize(entityDataKey);
            _entityData.Resource = new ItemType(_entityData.CreatureStats.Name, Random.Range(2, 5));
            RPCSetEntityDataServer(_entityData);
            //RPCSendCommandWaitServer();
        }
    }
    
    public ItemType PickUp()
    {
        CreatureTransform.gameObject.SetActive(false);
        GhostTransform.gameObject.SetActive(false);
        return _entityData.Resource;
    }

    public void Drop(GridManager.GridCoordinate position)
    {
        throw new System.NotImplementedException();
    }

    
    #region -- RPC Commands --
    
    [ServerRpc]
    void RPCSetEntityDataServer(ResourceEntityData data)
    {
        RPCSetEntityDataClient(data);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetEntityDataClient(ResourceEntityData data)
    {
        TickBased.Logger.Logger.Log("Setting Entity Data", "RPCSetEntityDataClient");
        SetEntityData(data);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RPCSendCommandWaitServer()
    {
        RPCSendCommandWaitClient();
    }

    [ObserversRpc]
    public void RPCSendCommandWaitClient()
    {
        var tick = ServiceLocator.Get<IServiceTickManager>();
        tick.QueueCommand(_entityData.UniqueID, new WaitCommand(), tick.CurrentTick);
    }

    [ServerRpc]
    public void RPCSendCommandMoveServer(GridManager.GridCoordinate newPos, bool useGhostPrediction)
    {
        RPCSendCommandMoveClient(newPos, useGhostPrediction);
    }

    [ObserversRpc]
    public void RPCSendCommandMoveClient(GridManager.GridCoordinate newPos, bool useGhostPrediction)
    {
        SetMovement(newPos, useGhostPrediction);
    }

    #endregion


}