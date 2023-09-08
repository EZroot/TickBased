using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FearProj.ServiceLocator
{
    public interface IServiceCreatureManager : IService
    {
        List<ICreatureEntity> AllCreaturesInScene { get; }
        ICreatureEntity GetCreature(string uniqueId);

        void AddCreature(ICreatureEntity data);
    }
}