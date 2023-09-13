using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FearProj.ServiceLocator
{
    public interface IServiceCreatureManager : IService
    {
        List<ICreatureEntity> AllCreaturesInScene { get; }
        List<ICreatureEntity> AllPlayersInScene { get; }
        ICreatureEntity GetCreature(string uniqueId);
        PlayerEntity GetPlayer(string uniqueId);

        void AddCreature(ICreatureEntity data);
    }
}