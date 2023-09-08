using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class CreatureManager : MonoBehaviour, IServiceCreatureManager
    {
        [ShowInInspector] private List<ICreatureEntity> _allCreaturesInScene;
        public List<ICreatureEntity> AllCreaturesInScene => _allCreaturesInScene;
        
        public ICreatureEntity GetCreature(string uniqueId)
        {
            for (var i = 0; i < _allCreaturesInScene.Count; i++)
            {
                if (_allCreaturesInScene[i].UniqueID == uniqueId)
                    return _allCreaturesInScene[i];
            }
            Logger.LogError($"Failed to Get Creature {uniqueId}", "CreatureManager");
            return null;
        }

        public void AddCreature(ICreatureEntity data)
        {
            _allCreaturesInScene ??= new List<ICreatureEntity>();
            Logger.Log($"Added {data.EntityData.CreatureStats.Name} {data.UniqueID} to list", "CreatureManager");
            _allCreaturesInScene.Add(data);
        }
    }
}