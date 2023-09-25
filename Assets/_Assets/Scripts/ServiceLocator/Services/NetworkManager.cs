using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Transporting;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class NetworkManager : MonoBehaviour, IServiceNetworkManager
    {
        [SerializeField] private FishNet.Managing.NetworkManager _fishnetNetworkManager;
        [SerializeField] private string _clientAddress = "192.168.0.4";
        [SerializeField] private bool _devServer = true;
        private string _bindServerAddress = "192.168.0.4";
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
            
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _bindServerAddress = ip.ToString();
                    break;
                }
            }
            _fishnetNetworkManager.TransportManager.Transport.SetServerBindAddress(_bindServerAddress,IPAddressType.IPv4);
            _fishnetNetworkManager.TransportManager.Transport.SetClientAddress(_devServer ? _bindServerAddress : _clientAddress);

//             _fishnetNetworkManager.TransportManager.Transport.SetServerBindAddress(GetLocalIPAddress(),
//                 IPAddressType.IPv4);
// #if UNITY_ANDROID && !UNITY_EDITOR
//             _fishnetNetworkManager.TransportManager.Transport.SetClientAddress(GetLocalIPAddress());
// #else
//                 
//                 _fishnetNetworkManager.TransportManager.Transport.SetClientAddress(GetLocalIPAddress());
// #endif
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
                for (var i = 0; i < _currentConnectedPlayerStats.Count; i++)
                {
                    var clientStat = _currentConnectedPlayerStats[i];
                    //already registered
                    if (clientStat.PlayerClientId == client.Key)
                    {
                        clientFound = true;
                        removeClientIndex = i;
                        break;
                    }
                }

                if (clientFound == false)
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

//     public string GetLocalIPAddress()
//     {
// #if UNITY_ANDROID && !UNITY_EDITOR
//         try
//         {
//             using (AndroidJavaObject multicastLock =
//  new AndroidJavaObject("android.net.wifi.WifiManager$MulticastLock", "lock"))
//             using (AndroidJavaObject wifiManager = new AndroidJavaObject("android.net.wifi.WifiManager"))
//             using (AndroidJavaObject wifiInfo = wifiManager.Call<AndroidJavaObject>("getConnectionInfo"))
//             {
//                 int ip = wifiInfo.Call<int>("getIpAddress");
//                 string localIP =
//  string.Format("{0}.{1}.{2}.{3}", (ip & 0xff), (ip >> 8 & 0xff), (ip >> 16 & 0xff), (ip >> 24 & 0xff));
//                 return localIP;
//             }
//         }
//         catch
//         {
//             return "Failed to retrieve local IP address on Android.";
//         }
// #elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
//         try
//         {
//             var host = Dns.GetHostEntry(Dns.GetHostName());
//             foreach (var ip in host.AddressList)
//             {
//                 if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
//                 {
//                     return ip.ToString();
//                 }
//             }
//
//             return "Local IP Address Not Found on Windows.";
//         }
//         catch
//         {
//             return "Failed to retrieve local IP address on Windows.";
//         }
// #endif
//         return "Unsupported platform.";
//     }

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