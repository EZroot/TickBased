using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Transporting;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class NetworkManager : MonoBehaviour, IServiceNetworkManager
    {
        [SerializeField] private FishNet.Managing.NetworkManager _fishnetNetworkManager;
        private List<CurrentConnectedPlayerStats> _currentConnectedPlayerStats;

        public FishNet.Managing.NetworkManager FishnetManager => _fishnetNetworkManager;
        public List<CurrentConnectedPlayerStats> CurrentConnectedPlayerStatsCollection => _currentConnectedPlayerStats;
        
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;

        void Start()
        {
            _currentConnectedPlayerStats = new List<CurrentConnectedPlayerStats>();
            _fishnetNetworkManager.ClientManager.OnClientConnectionState += OnClientStarted;
            _fishnetNetworkManager.ClientManager.OnRemoteConnectionState += PopulatePlayerList;
        }

        public void StartOrStopServer()
        {
            if (_fishnetNetworkManager == null)
                return;

            if (_serverState != LocalConnectionState.Stopped)
                _fishnetNetworkManager.ServerManager.StopConnection(true);
            else
                _fishnetNetworkManager.ServerManager.StartConnection();

            TickBased.Logger.Logger.Log($"Server Status: <color=blue>{_serverState.ToString()}</color>");
        }


        public void StartOrStopClient()
        {
            if (_fishnetNetworkManager == null)
                return;

            if (_clientState != LocalConnectionState.Stopped)
                _fishnetNetworkManager.ClientManager.StopConnection();
            else
            {    
                _fishnetNetworkManager.ClientManager.StartConnection();
            }
            TickBased.Logger.Logger.Log($"Server Status: <color=blue>{_clientState.ToString()}</color>");
        }

        void OnClientStarted(ClientConnectionStateArgs args)
        {
            // if (args.ConnectionState == LocalConnectionState.Started)
            // {
            //     StartCoroutine(LoadMainScene(SceneManager.SceneType.GameScene));
            // }
        }

        IEnumerator LoadMainScene(SceneManager.SceneType scene)
        {
            yield return LoadScene(scene);
        }

        async UniTask LoadScene(SceneManager.SceneType scene)
        {
            var scenemgr = ServiceLocator.Get<IServiceSceneManager>();
            await scenemgr.LoadSceneAddressableAsync(scene);
        }

        void PopulatePlayerList(RemoteConnectionStateArgs args)
        {
            foreach (var client in _fishnetNetworkManager.ClientManager.Clients)
            {
                bool clientFound = false;
                int removeClientIndex = -1;
                for(var i = 0; i < _currentConnectedPlayerStats.Count; i++)
                {
                    var clientStat = _currentConnectedPlayerStats[i];
                    //already registered
                    if(clientStat.PlayerClientId == client.Key)
                    {
                        clientFound = true;
                        removeClientIndex = i;
                        break;
                    }
                }

                if(clientFound == false)
                {
                    var playerStats = new CurrentConnectedPlayerStats(client.Key, client.Value);
                    _currentConnectedPlayerStats.Add(playerStats);

                }
                else
                {
                    _currentConnectedPlayerStats.RemoveAt(removeClientIndex);
                }
            }
        }
    }

    [System.Serializable]
    public class CurrentConnectedPlayerStats
    {
        [SerializeField] private int _playerClientId;
        private NetworkConnection _playerNetworkConnection;

        public int PlayerClientId => _playerClientId; 
        public NetworkConnection PlayerNetworkConnection => _playerNetworkConnection; 

        public CurrentConnectedPlayerStats(int clientid, NetworkConnection connection)
        {
            this._playerClientId = clientid;
            this._playerNetworkConnection = connection;
        }

    }
}