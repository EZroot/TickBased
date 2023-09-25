using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;
using FishNet.Connection;
using FishNet.Object;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Logger = TickBased.Logger.Logger;
using Random = UnityEngine.Random;

public class PlayerBuildController : NetworkBehaviour
{
    [SerializeField] private TMP_Text _buttonBuildText;
    private EntityType _typeToBuild;
    private string _typeToBuildDataID;
    private bool _buildMode = false;
    
    public void ToggleBuildMode()
    {
        _buildMode = !_buildMode;
        var tex = _buildMode ? "<color=green>Activated</color>" : "<color=red>Deactivated</color>";
        _buttonBuildText.text = $"Build Mode: {tex}";
    }
    void Start()
    {
        _typeToBuild = EntityType.Wall;
        _typeToBuildDataID = "dev_logwall";
        
        var tex = _buildMode ? "<color=green>Activated</color>" : "<color=red>Deactivated</color>";
        _buttonBuildText.text = $"Build Mode: {tex}";
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _buildMode)
        {
            var pmgr = ServiceLocator.Get<IServiceCreatureManager>();
            var gridMgr = ServiceLocator.Get<IServiceGridManager>();
            var player = pmgr.Player;
            var mousePos = gridMgr.GetGridPositionFromMouse(Camera.allCameras[0]);
            //can prob store blueprint requiremets in the item were tryin to build
            if (player.CreatureInventory.TryGetItem("Red Log", out var item))
            {
                var amountRequiredForWall = 2;
                if (item.Count >= amountRequiredForWall)
                {
                    var isTileEmpty = gridMgr.GetTile(mousePos.X, mousePos.Y).State == GridManager.TileState.Empty; 
                    if (isTileEmpty)
                    {
                        player.CreatureInventory.RemoveItem(item,amountRequiredForWall);

                        gridMgr.SetTileData(mousePos.X, mousePos.Y,
                            new GridManager.Tile(GridManager.TileState.Obstacle, null));
                        RPCLoadEntityServer(_typeToBuild, mousePos, _typeToBuildDataID);
                    }
                }
            }
        }
    }

    public void OnClick_SelectWall()
    {
        
    }

    public void OnBuild(GridManager.GridCoordinate coordinateToBuild)
    {
        
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
        var buildable = result as IBuildableObject;
        buildable.RPCSetBuildableDataKeyServer(dataKey, 1);
    }
}