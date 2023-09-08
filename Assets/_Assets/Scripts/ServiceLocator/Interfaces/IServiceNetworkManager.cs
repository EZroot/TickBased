using System.Collections.Generic;

namespace FearProj.ServiceLocator
{
    public interface IServiceNetworkManager : IService
    {
        public List<CurrentConnectedPlayerStats> CurrentConnectedPlayerStatsCollection {get;}
        FishNet.Managing.NetworkManager FishnetManager {get;}
        void StartOrStopServer();
        void StartOrStopClient();
    }
}