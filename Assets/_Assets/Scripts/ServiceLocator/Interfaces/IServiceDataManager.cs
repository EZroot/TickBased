using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FearProj.ServiceLocator
{
    public interface IServiceDataManager : IService
    {
        void TestFunc();
        UniTask LoadAndProcessData();
        List<T> GetEntityData<T>(Type type) where T : EntityData;
        UniTask SaveDataAsJson<T>(T data) where T : EntityData;
        UniTask<T> LoadDataFromJson<T>(string fileName) where T : EntityData;
        Type GetEntityDataType(string id);
    }
}
