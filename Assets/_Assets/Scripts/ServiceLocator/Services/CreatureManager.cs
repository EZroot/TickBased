using System.Collections.Generic;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class CreatureManager : MonoBehaviour, IServiceCreatureManager
    {
        private List<ICreatureEntity> _allCreaturesInScene;
        private List<ICreatureEntity> _allPlayersInScene;
        private PlayerEntity _player;
        public List<ICreatureEntity> AllCreaturesInScene => _allCreaturesInScene;
        public List<ICreatureEntity> AllPlayersInScene => _allPlayersInScene;
        public PlayerEntity Player => _player;

        public void SetPlayer(PlayerEntity player)
        {
            _player = player;
        }
        public ICreatureEntity GetCreature(string uniqueId)
        {
            for (var i = 0; i < _allCreaturesInScene.Count; i++)
            {
                TickBased.Logger.Logger.Log($"Creatures ID {_allCreaturesInScene[i].UniqueID}", "CreatureManager");
                if (_allCreaturesInScene[i].UniqueID == uniqueId)
                    return _allCreaturesInScene[i];
            }
            TickBased.Logger.Logger.LogError($"Failed to Get Creature {uniqueId}", "CreatureManager");
            return null;
        }

        public PlayerEntity GetPlayer(string uniqueId)
        {
            for (var i = 0; i < _allPlayersInScene.Count; i++)
            {
                TickBased.Logger.Logger.Log($"GetPlayer: Player ID [{_allPlayersInScene[i].UniqueID}]", "CreatureManager");
                if (_allPlayersInScene[i].UniqueID == uniqueId)
                    return _allPlayersInScene[i] as PlayerEntity;
            }
            TickBased.Logger.Logger.LogError($"GetPlayer: Failed to Get Player [{uniqueId}]", "CreatureManager");
            return null;        }


        public void AddCreature(ICreatureEntity data)
        {
            _allCreaturesInScene ??= new List<ICreatureEntity>();
            _allPlayersInScene ??= new List<ICreatureEntity>();
            TickBased.Logger.Logger.Log($"Added [{data.EntityData.CreatureStats.Name} {data.UniqueID}] to list", "CreatureManager");
            
            if(data.EntityData.IsPlayer)
                _allPlayersInScene.Add(data);
            
            _allCreaturesInScene.Add(data);
        }
    }
}