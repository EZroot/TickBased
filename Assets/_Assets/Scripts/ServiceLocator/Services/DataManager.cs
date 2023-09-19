using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FearProj.ServiceLocator
{
    public class DataManager : IServiceDataManager
    {

        // Dictionary to store loaded DataSOs
        private List<DataSO> _entityDataSOCollection = null;
        private Dictionary<Type, List<EntityData>> _entityDataDictionary = null;

        public DataManager()
        {
            _entityDataSOCollection = new List<DataSO>();
            _entityDataDictionary = new Dictionary<Type, List<EntityData>>();

            var dataSettings = ServiceLocator.Get<IServiceGameManager>().GameSettings.DataSettings;
            TickBased.Utils.FileUtils.InitDirectories(dataSettings);

            TickBased.Logger.Logger.Log("Data Manager Called!", "DataManager");
            //LoadAndProcessData().Forget();
        }

        public List<T> GetEntityData<T>(Type type) where T : EntityData
        {
            List<T> _shallowList = new List<T>();
            foreach (var data in _entityDataDictionary[type])
            {
                _shallowList.Add(data as T);
            }
            return _shallowList;
        }

        public async UniTask LoadAndProcessData()
        {
            // Create a dictionary to map each type to an action
            Dictionary<Type, Func<DataSO, UniTask>> typeActions = new Dictionary<Type, Func<DataSO, UniTask>>
        {
            { typeof(PlayerDataSO), async(data) => await PlayerData_Processor(data) },
            { typeof(ObjectEntityDataSO), async(data) => await ObjectData_Processor(data) },
            { typeof(CreatureDataSO), async(data) => await CreatureData_Processor(data) }

        };
            var label = ServiceLocator.Get<IServiceGameManager>().GameSettings.DataSettings.AddressableEntityDataSettingLabel;
            IList<DataSO> dataSOList = await Addressables.LoadAssetsAsync<DataSO>(label, DataSO_OnLoad).Task;

            // Add more types and their corresponding actions as needed
            foreach (var baseSO in dataSOList)
            {
                // Get the type of the current ScriptableObject
                var baseSOType = baseSO.GetType();
                TickBased.Logger.Logger.Log($"Loading {baseSOType}", "DataManager");

                // Check if the type is present in the dictionary, and execute the corresponding action
                if (typeActions.TryGetValue(baseSOType, out Func<DataSO, UniTask> action))
                {
                    await action(baseSO);
                }
                else
                {
                    // Handle the base class or any unknown types here
                    TickBased.Logger.Logger.Log("ProcessData: Found DataSO or unknown type", "DataManager");
                }
            }
        }

        private async UniTask PlayerData_Processor(DataSO baseSO)
        {
            var playerSO = (PlayerDataSO)baseSO;
            var entityData = playerSO.EntityData;

            //Initial Copy of data
            var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
            //Check if saved data exists
            var fileName = dataObj.ID;
            var loadedData = await LoadDataFromJson<PlayerEntityData>(fileName);

            //No saved data exists
            if (loadedData == null)
                await SaveDataAsJson(dataObj);
            else
                dataObj = loadedData;
            AddEntityDataToDictionary(dataObj.GetType(), dataObj);
            // Do something with the EntityDataA
            TickBased.Logger.Logger.Log("PlayerData_Processor: <color=green>Loaded</color>", "DataManager");
        }

        private async UniTask ObjectData_Processor(DataSO baseSO)
        {
            var dataSO = (ObjectEntityDataSO)baseSO;
            var entityData = dataSO.EntityData;

            var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
            //Check if saved data exists
            var fileName = dataObj.ID;
            var loadedData = await LoadDataFromJson<ObjectEntityData>(fileName);

            //No saved data exists
            if (loadedData == null)
                await SaveDataAsJson(dataObj);
            else
                dataObj = loadedData;

            AddEntityDataToDictionary(dataObj.GetType(), dataObj);
            // Do something with the EntityDataB
            TickBased.Logger.Logger.Log("ObjectData_Processor: <color=green>Loaded</color>", "DataManager");
        }

        private async UniTask CreatureData_Processor(DataSO baseSO)
        {
            var dataSO = (CreatureDataSO)baseSO;
            var entityData = dataSO.CreatureEntityData;

            var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
            //Check if saved data exists
            var fileName = dataObj.ID;
            var loadedData = await LoadDataFromJson<CreatureEntityData>(fileName);

            //No saved data exists
            if (loadedData == null)
                await SaveDataAsJson(dataObj);
            else
                dataObj = loadedData;
            
            AddEntityDataToDictionary(dataObj.GetType(), dataObj);
            // Do something with the EntityDataB
            TickBased.Logger.Logger.Log("CreatureData_Processor: <color=green>Loaded</color>", "DataManager");
        }

        public async UniTask SaveDataAsJson<T>(T data) where T : EntityData
        {
            var json = JsonUtility.ToJson(data);
            await TickBased.Utils.FileUtils.SaveFile(json, data.ID);
        }

        public async UniTask<T> LoadDataFromJson<T>(string fileName) where T : EntityData
        {
            var data = await TickBased.Utils.FileUtils.LoadFile<T>(fileName);
            return data;
        }


        private void AddEntityDataToDictionary(Type type, EntityData data)
        {
            if (_entityDataDictionary.TryGetValue(type, out List<EntityData> values))
            {
                TickBased.Logger.Logger.Log($"AddEntityDataToDictionary: Added {type} to existing Dictionary Data", "DataManager");
                values.Add(data);
            }
            else
            {
                TickBased.Logger.Logger.Log($"AddEntityDataToDictionary: Created {type} in Dictionary Data", "DataManager");
                _entityDataDictionary.Add(type, new List<EntityData>() { data }); // Add a new empty list for the ID
            }
        }

        private void DataSO_OnLoad(DataSO data)
        {
            // Process the loaded DataSO object here
            // For example:
            TickBased.Logger.Logger.Log($"DataSO_OnLoad: addressable SO asset loaded {data.name}", "DataManager");
        }

        public void TestFunc()
        {
            TickBased.Logger.Logger.Log("DataManager Tested and working", "DataManager");
        }

    }
}