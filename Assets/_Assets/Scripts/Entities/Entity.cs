using System;
using FishNet.Object;
using FearProj.ServiceLocator;
using UnityEngine;
public class Entity<TData> : NetworkBehaviour, ITileObject where TData : EntityData
{
    public delegate void OnEntityDataChangedDelegate(TData data);
    public event OnEntityDataChangedDelegate OnEntityDataChanged;
    [SerializeField] protected string _entityDataKey;
    [SerializeField] protected TData _entityData;
    private GridManager.GridCoordinate _gridCoordinates;
    public TData EntityData => _entityData;
    public GridManager.GridCoordinate GridCoordinates => _gridCoordinates;
    
    public virtual void Initialize()
    {
        var dataMngr = ServiceLocator.Get<IServiceDataManager>();
        var data = dataMngr.GetEntityData<TData>(typeof(TData));
        if (data.Count > 0)
        {
            foreach (var tmp in data)
            {
                if (tmp.ID == _entityDataKey)
                {
                    TickBased.Logger.Logger.Log($"Entity Data: <color=green>[{_entityDataKey}]{tmp.GetType()}</color> assigned to <color=green>{this.GetType()}</color>");
                    SetEntityData(tmp);
                    break;
                }
            }
        }
        GenerateUniqueID();
    }
    
    public virtual void Initialize(string entityDataKey)
    {
        var dataMngr = ServiceLocator.Get<IServiceDataManager>();
        var data = dataMngr.GetEntityData<TData>(typeof(TData));
        if (data.Count > 0)
        {
            foreach (var tmp in data)
            {
                if (tmp.ID == entityDataKey)
                {
                    TickBased.Logger.Logger.Log($"Entity Data: <color=green>[{entityDataKey}]{tmp.GetType()}</color> assigned to <color=green>{this.GetType()}</color>");
                    SetEntityData(tmp);
                    break;
                }
            }
        }
        GenerateUniqueID();
    }

    public virtual void SetEntityData(TData data)
    {
        _entityData = data;
        if (OnEntityDataChanged != null)
            OnEntityDataChanged(_entityData);
    }

    public virtual void SaveEntityData(TData data)
    {
        if(!base.IsOwner)
            return;
            
        var dataMngr = ServiceLocator.Get<IServiceDataManager>();
        _ = dataMngr.SaveDataAsJson<TData>(data);
    }

    public void OnTileEnter()
    {
        throw new System.NotImplementedException();
    }

    public void OnTileExit()
    {
        throw new System.NotImplementedException();
    }

    public void SetGridCoordinates(int x, int y)
    {
        var gridManager = ServiceLocator.Get<IServiceGridManager>();

        //removing previous tile refs
        gridManager.SetTileData(_gridCoordinates.X,_gridCoordinates.Y, new GridManager.Tile(GridManager.TileState.Empty, null));
        _gridCoordinates = new GridManager.GridCoordinate(x, y);
        gridManager.SetTileData(_gridCoordinates.X,_gridCoordinates.Y, new GridManager.Tile(GridManager.TileState.Object, this));

    }
    
    protected void GenerateUniqueID()
    {
        var id = Guid.NewGuid().ToString();
        TickBased.Logger.Logger.Log($"Generating unique ID {id}", "CreatureEntity");
        EntityData.UniqueID = id;
    }
    
    [ServerRpc(RequireOwnership =  false)]
    public void RPCSetEntityDataKeyServer(string dataKey)
    {
        TickBased.Logger.Logger.Log("Setting asdasd Data", "RPCSetEntityDataClient");

        RPCSetEntityDataKeyClient(dataKey);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetEntityDataKeyClient(string dataKey)
    {
        TickBased.Logger.Logger.Log("Setting asdasd client Data", "RPCSetEntityDataClient");
        Initialize(dataKey);
    }
}