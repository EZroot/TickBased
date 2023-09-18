using System;
using FearProj.ServiceLocator;
using FishNet.Object;
using UnityEngine;


public class PlayerGridController : NetworkBehaviour
{
    private void Start()
    {
        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        gridManager.OnPreGridGeneration += OnPreGridGeneration_LoadingScreen;
        gridManager.OnPostGridGeneration += OnPostGridGeneration_LoadingScreen;
    }

    void OnPreGridGeneration_LoadingScreen()
    {
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.ShowLoadingScreen("Generating World...");
    }
    
    void OnPostGridGeneration_LoadingScreen()
    {
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.HideLoadingScreen();
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner && IsHost)
        {
            var gridManager = ServiceLocator.Get<IServiceGridManager>();
            var seed = gridManager.GridSeed;
            var size = gridManager.GridSize;
            var tilesize = gridManager.GridTileSize;
            RPCGenerateGridBySeedServer(seed, new GridManager.GridData(size, size, tilesize));
        }
    }
    
    [ServerRpc]
    public void RPCGenerateGridBySeedServer(int seed, GridManager.GridData gridData)
    {
        RPCGenerateGridBySeedClient(seed, gridData);
    }
    
    [ObserversRpc(BufferLast = true)]
    public void RPCGenerateGridBySeedClient(int seed, GridManager.GridData gridData)
    {
        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        gridManager.InitializeGridBySeed(seed, gridData);
    }
}