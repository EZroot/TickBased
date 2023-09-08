using System.Collections;
using Cysharp.Threading.Tasks;
using FishNet.Transporting;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

namespace FearProj.ServiceLocator
{
    public class NetworkSpawner : MonoBehaviour, IServiceNetworkSpawner
    {
        //[SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private FishNet.Managing.NetworkManager _fishnetNetworkManager;
        public FishNet.Managing.NetworkManager FishnetManager => _fishnetNetworkManager;

        public NetworkObject SpawnPlayer(NetworkConnection ownerConnection)
        {
            // Logger.Log("Spawning player - Network Spawner");
            // var go = Instantiate(_playerPrefab);
            // _fishnetNetworkManager.ServerManager.Spawn(go, ownerConnection);
            // go.GiveOwnership(ownerConnection);
            return null;
        }
    }
}