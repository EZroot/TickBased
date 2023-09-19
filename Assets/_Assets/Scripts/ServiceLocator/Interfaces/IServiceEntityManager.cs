using Cysharp.Threading.Tasks;
using FishNet.Connection;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public interface IServiceEntityManager : IService
    {
        UniTask<Entity<T>> LoadEntity<T>(EntityType entityType, GridManager.GridCoordinate gridCoordinate)
            where T : EntityData;
    }
}