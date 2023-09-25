using System;
using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using FishNet.Object;
using TickBased.Scripts.Commands;
using UnityEngine;

public class WallEntity : CreatureEntity<WallEntityData>, IBuildableObject
{

    private bool _isConstructionCompleted = false;
    public bool IsConstructionCompleted => _isConstructionCompleted;
    public event Action OnConstructionComplete;
    public int WorkRequired => _entityData.RequiredWork;
    public int CurrentWork => _entityData.CurrentWork;
    public override bool CanInteract => _isConstructionCompleted == false;

    public override void Start()
    {
        base.Start();
        OnEntityDataChanged += SaveEntityData;
        OnEntityDataChanged += OnDataChanged_UpdateSpriteOnWork;
    }
    void OnDestroy()
    {
        //need to remove from creature manager
        OnEntityDataChanged -= SaveEntityData;
    }

    void OnDataChanged_UpdateSpriteOnWork(WallEntityData data)
    {
        float normalizedWork = (float)Math.Floor(((float)data.CurrentWork / (float)data.RequiredWork) * 4f) / 4f;
        UpdateWearableAlpha(normalizedWork);

        if (_entityData.CurrentWork >= _entityData.RequiredWork)
        {
            _isConstructionCompleted = true;
            SetGridCoordinates(GridCoordinates.X,_gridCoordinates.Y, GridManager.TileState.Obstacle);
        }
    }
    
    public void Initialize(string dataKey, int startingWorkLevel)
    {
        SetGridCoordinates(GridCoordinates.X,_gridCoordinates.Y, GridManager.TileState.Object);
        if (IsOwner)
        {
            base.Initialize(dataKey);
            _entityData.CurrentWork = startingWorkLevel;
            RPCSetEntityDataServer(_entityData);
        }
        
    }

    public void AddWork(int workAmountToAddToBuild)
    {
        _entityData.CurrentWork += workAmountToAddToBuild;
        RPCSetEntityDataServer(_entityData);
    }
    
    public override void Initialize(string entityDataKey)
    {
        if (base.IsOwner)
        {
            base.Initialize(entityDataKey);
            
            RPCSetEntityDataServer(_entityData);
            //RPCSendCommandWaitServer();
        }
    }
    
    [ServerRpc(RequireOwnership =  false)]
    public void RPCSetBuildableDataKeyServer(string dataKey, int startingWork)
    {
        TickBased.Logger.Logger.Log("Setting asdasd Data", "RPCSetEntityDataClient");

        RPCSetBuildableDataKeyClient(dataKey,startingWork);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetBuildableDataKeyClient(string dataKey, int startingWork)
    {
        TickBased.Logger.Logger.Log($"Setting {dataKey} client Data", "RPCSetEntityDataClient");
        Initialize(dataKey,startingWork);
    }
    

    
    #region -- RPC Commands --
    
    [ServerRpc]
    void RPCSetEntityDataServer(WallEntityData data)
    {
        RPCSetEntityDataClient(data);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetEntityDataClient(WallEntityData data)
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