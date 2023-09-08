using FishNet.Connection;
using FishNet.Object;
using FearProj.ServiceLocator;
using UnityEngine;

public class PlayerEntity : CreatureEntity<PlayerEntityData>
{
    [SerializeField] private DisableNetworkObjects _disableNetworkObjects;
    void Awake()
    {
        var sceneManager = ServiceLocator.Get<IServiceSceneManager>();
        sceneManager.OnSceneFinishedLoading += On_SceneLoad;
    }

    void Start()
    {
        var playerManager = ServiceLocator.Get<IServicePlayerManager>();

        playerManager.AddPlayer(this);

        OnEntityDataChanged += SaveEntityData;
    }

    void Update()
    {
        if (base.IsOwner)
        {
            if(Input.GetKey(KeyCode.D))
                SetMovement(Vector2.right, 2f);

            if (Input.GetKeyDown(KeyCode.A))
            {
                _entityData.CreatureSprites.SpriteAddressableKey = "warforgedmage";
                RPCSetPlayerDataServer(_entityData);
            }
        }
    }

    void OnDestroy()
    {
        var playerManager = ServiceLocator.Get<IServicePlayerManager>();
        playerManager.RemovePlayer(this);
        OnEntityDataChanged -= SaveEntityData;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Initialize();
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
    }

    public override void Initialize()
    {
        if (base.IsOwner)
        {
            base.Initialize();
            var netwrk = ServiceLocator.Get<IServiceNetworkManager>();
            var connection = netwrk.FishnetManager.ClientManager.Connection;

            var playerManager = ServiceLocator.Get<IServicePlayerManager>();
            playerManager.SetClientStats(connection.ClientId, playerManager.ClientStats.Username);

            _entityData.SetClientStats(playerManager.ClientStats);
            
            RPCSetPlayerDataServer(_entityData);

            var sceneManager = ServiceLocator.Get<IServiceSceneManager>();
            sceneManager.LoadSceneCoroutine(FearProj.ServiceLocator.SceneManager.SceneType.GameScene);
        }
    }

    public void On_SceneLoad(FearProj.ServiceLocator.SceneManager.SceneType sceneType)
    {
        if (base.IsOwner)
        {
            if (sceneType == FearProj.ServiceLocator.SceneManager.SceneType.GameScene)
            {
                Logger.Log("On Scene Load - Enabling Player Body", "PlayerEntity");
                RPCSetPlayerBodyEnabledServer(true);
            }
            else
            {
                Logger.Log("On Scene Load - Disabling Player Body", "PlayerEntity");
                RPCSetPlayerBodyEnabledServer(false);
            }
        }
    }
    
    [ServerRpc]
    void RPCSetPlayerDataServer(PlayerEntityData data)
    {
        RPCSetPlayerDataClient(data);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetPlayerDataClient(PlayerEntityData data)
    {
        Logger.Log("Setting Entity Data", "RPCSetPlayerDataClient");
        SetEntityData(data);
    }
    
    [ServerRpc]
    void RPCSetPlayerBodyEnabledServer(bool isEnabled)
    {
        RPCSetPlayerBodyEnabledClient(isEnabled);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetPlayerBodyEnabledClient(bool isEnabled)
    {
        _disableNetworkObjects.gameObject.SetActive(isEnabled);

        if (isEnabled)
        {
            _disableNetworkObjects.EnableOwnerObjects(base.IsOwner);
        }
    }

    // [ServerRpc]
    // void RPCSpawnPlayer(NetworkConnection connection)
    // {
    //     Logger.Log($"Spawned with connection: {connection.ClientId}");
    //     var spawner = ServiceLocator.Get<IServiceNetworkSpawner>();
    //     var obj = spawner.SpawnPlayer(connection);
    // }
}