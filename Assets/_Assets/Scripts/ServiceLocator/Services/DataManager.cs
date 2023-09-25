using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = TickBased.Logger.Logger;

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

        public Type GetEntityDataType(string id)
        {
            foreach (var key in _entityDataDictionary)
            {
                var dataList = key.Value;
                foreach(var data in dataList)
                {
                    if (data.ID == id)
                        return key.Key;
                }
            }
            Logger.LogError($"Failed to get data type by id: {id}", "DataManager");
            return null;
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
            { typeof(AggressiveDataSO), async(data) => await AggressiveEntityData_Processor(data) },
            { typeof(WallDataSO), async(data) => await WallData_Processor(data) },
            { typeof(ResourceDataSO), async(data) => await ResourceData_Processor(data) }
            //{ typeof(AggressiveDataSO), async(data) => await ProcessData<AggressiveEntityData>(data) }

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
                    TickBased.Logger.Logger.Log("ProcessData: Found DataSO or unknown type. DID YOU FORGET TO ADD IT TO PROCESSOR/ADDRESSABLES?", "DataManager");
                }
            }
        }
        
        // private async UniTask ProcessData<T>(DataSO baseSO) where T : EntityData
        // {
        //     // Cast the baseSO to its actual type, bypassing the need for dynamic.
        //     T entityData = (baseSO as dynamic).EntityData as T;
        //
        //     if (entityData == null)
        //     {
        //         // Log an error message if casting failed.
        //         TickBased.Logger.Logger.LogError($"Failed to cast EntityData to {typeof(T).Name}", "DataManager");
        //         return;
        //     }
        //
        //     // DeepCopy the entityData
        //     var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
        //
        //     //Check if saved data exists
        //     var fileName = dataObj.ID;
        //     var loadedData = await LoadDataFromJson<T>(fileName);
        //
        //     //No saved data exists
        //     if (loadedData == null)
        //     {
        //         await SaveDataAsJson(dataObj);
        //     }
        //     else
        //     {
        //         dataObj = loadedData;
        //     }
        //
        //     //Add to dictionary
        //     AddEntityDataToDictionary(dataObj.GetType(), dataObj);
        //
        //     //Log completion
        //     TickBased.Logger.Logger.Log($"{typeof(T).Name}_Processor: <color=green>Loaded</color>", "DataManager");
        // }

        
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
        
        private async UniTask ResourceData_Processor(DataSO baseSO)
        {
            var playerSO = (ResourceDataSO)baseSO;
            var entityData = playerSO.EntityData;
        
            //Initial Copy of data
            var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
            //Check if saved data exists
            var fileName = dataObj.ID;
            var loadedData = await LoadDataFromJson<ResourceEntityData>(fileName);
        
            //No saved data exists
            if (loadedData == null)
                await SaveDataAsJson(dataObj);
            else
                dataObj = loadedData;
            AddEntityDataToDictionary(dataObj.GetType(), dataObj);
            // Do something with the EntityDataA
            TickBased.Logger.Logger.Log("ResourceData_Processor: <color=green>Loaded</color>", "DataManager");
        }
        
        private async UniTask AggressiveEntityData_Processor(DataSO baseSO)
        {
            var playerSO = (AggressiveDataSO)baseSO;
            var entityData = playerSO.EntityData;
        
            //Initial Copy of data
            var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
            //Check if saved data exists
            var fileName = dataObj.ID;
            var loadedData = await LoadDataFromJson<AggressiveEntityData>(fileName);
        
            //No saved data exists
            if (loadedData == null)
                await SaveDataAsJson(dataObj);
            else
                dataObj = loadedData;
            AddEntityDataToDictionary(dataObj.GetType(), dataObj);
            // Do something with the EntityDataA
            TickBased.Logger.Logger.Log("AggressiveEntityData_Processor: <color=green>Loaded</color>", "DataManager");
        }
        
        private async UniTask WallData_Processor(DataSO baseSO)
        {
            var dataSO = (WallDataSO)baseSO;
            var entityData = dataSO.EntityData;
        
            var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
            //Check if saved data exists
            var fileName = dataObj.ID;
            var loadedData = await LoadDataFromJson<WallEntityData>(fileName);
        
            //No saved data exists
            if (loadedData == null)
                await SaveDataAsJson(dataObj);
            else
                dataObj = loadedData;
        
            AddEntityDataToDictionary(dataObj.GetType(), dataObj);
            // Do something with the EntityDataB
            TickBased.Logger.Logger.Log("WallData_Processor: <color=green>Loaded</color>", "DataManager");
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
        //
        // private async UniTask CreatureData_Processor(DataSO baseSO)
        // {
        //     var dataSO = (CreatureDataSO)baseSO;
        //     var entityData = dataSO.CreatureEntityData;
        //
        //     var dataObj = TickBased.Utils.DataUtils.DeepCopy(entityData);
        //     //Check if saved data exists
        //     var fileName = dataObj.ID;
        //     var loadedData = await LoadDataFromJson<CreatureEntityData>(fileName);
        //
        //     //No saved data exists
        //     if (loadedData == null)
        //         await SaveDataAsJson(dataObj);
        //     else
        //         dataObj = loadedData;
        //     
        //     AddEntityDataToDictionary(dataObj.GetType(), dataObj);
        //     // Do something with the EntityDataB
        //     TickBased.Logger.Logger.Log("CreatureData_Processor: <color=green>Loaded</color>", "DataManager");
        // }

        public async UniTask SaveDataAsJson<T>(T data) where T : EntityData
        {
            var json = JsonUtility.ToJson(data);
           // await TickBased.Utils.FileUtils.SaveFile(json, data.ID);
        }

        public async UniTask<T> LoadDataFromJson<T>(string fileName) where T : EntityData
        {
            var data = await TickBased.Utils.FileUtils.LoadFile<T>(fileName);
            Logger.Log($"{fileName} Data Loaded: {data!=null}");
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