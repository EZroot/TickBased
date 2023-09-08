using UnityEngine;
using System;
using System.Collections.Generic;
using FishNet.Connection;

namespace FearProj.ServiceLocator
{
    public class PlayerManager : MonoBehaviour, IServicePlayerManager
    {
        public delegate void OnPlayerClientAddDelegate(PlayerEntity playerEntity);
        public event OnPlayerClientAddDelegate OnPlayerClientAdded;
        public delegate void OnPlayerClientStatsChangedDelegate(ClientStats clientStats);
        public event OnPlayerClientStatsChangedDelegate OnPlayerClientStatsChanged;

        private List<PlayerEntity> _playerCollection;
        private ClientStats _clientStats;

        public List<PlayerEntity> PlayerCollection => _playerCollection;
        public ClientStats ClientStats => _clientStats;

        void Start()
        {
            _playerCollection = new List<PlayerEntity>();
        }

        public void AddPlayer(PlayerEntity playerEntity)
        {
            _playerCollection.Add(playerEntity);
            if(OnPlayerClientAdded != null)
                OnPlayerClientAdded(playerEntity);
        }

        public void RemovePlayer(PlayerEntity playerEntity)
        {
            var entityFound = false;
            for (var i = 0; i < _playerCollection.Count; i++)
            {
                if (_playerCollection[i].EntityData.ID == playerEntity.EntityData.ID)
                {
                    _playerCollection.RemoveAt(i);
                    break;
                }
            }

            if (entityFound == false)
            {
                _playerCollection.Add(playerEntity);
            }
        }

        public void TestFunc()
        {
            Logger.Log("Player Manager called");
        }

        public void SetClientStats(int clientId, string username)
        {
            Logger.Log($"[PlayerManager] SetClientStats -> ClientID {clientId} Username {username}");
            _clientStats = new ClientStats(clientId, username);
            if(OnPlayerClientStatsChanged != null)
                OnPlayerClientStatsChanged(_clientStats);
        }
    }
}