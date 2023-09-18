using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FearProj.ServiceLocator;
using UnityEngine;

public class BootstrapGame : MonoBehaviour
{
    public async void Start()
    {
        await LoadGame();
    }

    async UniTask LoadGame()
    {
        var datamgr = ServiceLocator.Get<IServiceDataManager>();
        await datamgr.LoadAndProcessData();
        TickBased.Logger.Logger.Log("Bootstrap: Data loaded success.", "BootstrapGame LoadGame");
        
        var sceneMgr = ServiceLocator.Get<IServiceSceneManager>();
        await sceneMgr.LoadSceneAddressableAsync(SceneManager.SceneType.LobbyScene);
    }
}
