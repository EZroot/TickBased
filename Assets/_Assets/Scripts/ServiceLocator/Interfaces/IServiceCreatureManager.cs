using System.Collections.Generic;

namespace FearProj.ServiceLocator
{
    public interface IServiceCreatureManager : IService
    {
        List<ICreatureEntity> AllCreaturesInScene { get; }
        List<ICreatureEntity> AllPlayersInScene { get; }
        ICreatureEntity GetCreature(string uniqueId);
        PlayerEntity GetPlayer(string uniqueId);
        PlayerEntity Player { get; }
        void AddCreature(ICreatureEntity data);
        void SetPlayer(PlayerEntity player);
    }
}