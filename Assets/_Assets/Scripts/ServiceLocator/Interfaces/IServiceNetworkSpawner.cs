using FishNet.Connection;
using FishNet.Object;

namespace FearProj.ServiceLocator
{
    public interface IServiceNetworkSpawner : IService
    {
        NetworkObject SpawnPlayer(NetworkConnection ownerConnection);
    }
}