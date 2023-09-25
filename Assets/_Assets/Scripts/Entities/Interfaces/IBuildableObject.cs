
    using System;

    public interface IBuildableObject
    {
        void Initialize(string dataKey, int startingWorkLevel);
        void AddWork(int workAmountToAddToBuild);
        int WorkRequired { get; }
        int CurrentWork { get; }
        bool IsConstructionCompleted { get; }
        event Action OnConstructionComplete;

        public void RPCSetBuildableDataKeyServer(string dataKey, int startingWork);
    }
