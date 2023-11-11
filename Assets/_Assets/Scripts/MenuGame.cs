using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

public class MenuGame : MonoBehaviour
{
 [SerializeField] private TMP_InputField _usernameInputField;

    void Start()
    {
        var netwrk = ServiceLocator.Get<IServiceNetworkManager>();
        netwrk.FishnetManager.ClientManager.OnClientConnectionState += OnConnectedClient;
        
        //StartServer();
    }
    public void StartServer()
    {
        var netwrk = ServiceLocator.Get<IServiceNetworkManager>();
        netwrk.StartOrStopServer();
    }

    public void StartClient()
    {
        var netwrk = ServiceLocator.Get<IServiceNetworkManager>();

        var connection = netwrk.FishnetManager.ClientManager.Connection;
        var playerManager = ServiceLocator.Get<IServicePlayerManager>();
        playerManager.SetClientStats(connection.ClientId, _usernameInputField.text);

        netwrk.StartOrStopClient();

    } 
    void OnConnectedClient(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            var netwrk = ServiceLocator.Get<IServiceNetworkManager>();
            var playerMgr = ServiceLocator.Get<IServicePlayerManager>();
            var connection = netwrk.FishnetManager.ClientManager.Connection;
            var clientId = connection.ClientId;
            var address = connection.GetAddress();
            var username = _usernameInputField.text;
            TickBased.Logger.Logger.Log($"OnConnectedClient: {username} {clientId} {address}");
        }
    }
}
