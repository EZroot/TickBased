using System;
using FishNet.Object;
using FearProj.ServiceLocator;
using TickBased.Utils;
using UnityEngine;
public class Entity<TData> : NetworkBehaviour, ITileObject where TData : EntityData
{
    public delegate void OnEntityDataChangedDelegate(TData data);
    public event OnEntityDataChangedDelegate OnEntityDataChanged;
    [SerializeField] protected string _entityDataKey;
    [SerializeField] protected TData _entityData;
    [Header("- DEBUG - dont change this")]
    [SerializeField] protected GridManager.GridCoordinate _gridCoordinates;
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
                    var deepcopydata = DataUtils.DeepCopy(tmp);
                    TickBased.Logger.Logger.Log($"Entity Data: <color=green>[{_entityDataKey}]{tmp.GetType()}</color> assigned to <color=green>{this.GetType()}</color>");
                    SetEntityData(deepcopydata);
                    break;
                }
            }
        }
        GenerateUniqueID();
    }
    
    public virtual void Initialize(string entityDataKey)
    {
        Debug.Log($"INITIALIZING ENTITY {GetType()}");
        var dataMngr = ServiceLocator.Get<IServiceDataManager>();
        var data = dataMngr.GetEntityData<TData>(typeof(TData));
        if (data.Count > 0)
        {
            foreach (var tmp in data)
            {
                if (tmp.ID == entityDataKey)
                {
                    var deepcopydata = DataUtils.DeepCopy(tmp);
                    //deepcopydata.UniqueID = uniqueID;
                    SetEntityData(deepcopydata);
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

    public void SetGridCoordinates(int x, int y, GridManager.TileState tileState)
    {
        var gridManager = ServiceLocator.Get<IServiceGridManager>();

        //removing previous tile refs
        gridManager.SetTileData(_gridCoordinates.X,_gridCoordinates.Y, new GridManager.Tile(GridManager.TileState.Empty, null));
        PathFinder.occupiedSquares.Remove(_gridCoordinates);
        _gridCoordinates = new GridManager.GridCoordinate(x, y);
        PathFinder.occupiedSquares.Add(_gridCoordinates);
        gridManager.SetTileData(_gridCoordinates.X,_gridCoordinates.Y, new GridManager.Tile(tileState, this));

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
        TickBased.Logger.Logger.Log($"Setting {dataKey} client Data", "RPCSetEntityDataClient");
        Initialize(dataKey);
    }
}