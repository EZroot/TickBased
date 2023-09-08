using FishNet.Connection;
using FishNet.Object;
using FearProj.ServiceLocator;
using UnityEngine;

public class PlayerEntity : CreatureEntity<PlayerEntityData>
{
    [SerializeField] private DisableNetworkObjects _disableNetworkObjects;
    [SerializeField] private GameObject[] _objectsInRange;
    void Awake()
    {
        var sceneManager = ServiceLocator.Get<IServiceSceneManager>();
        sceneManager.OnSceneFinishedLoading += On_SceneLoad;
    }

    public override void Start()
    {
        base.Start();
        Logger.Log("Start called", "PlayerEntity");
        var playerManager = ServiceLocator.Get<IServicePlayerManager>();

        playerManager.AddPlayer(this);

        OnEntityDataChanged += SaveEntityData;

        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.OnTick += OnTick_Collision;
    }

    void Update()
    {
        if (base.IsOwner)
        {
            if(Input.GetKeyDown(KeyCode.D))
                RPCSendCommandMoveServer(Vector2.right);
            else if(Input.GetKeyDown(KeyCode.A))
                RPCSendCommandMoveServer(Vector2.left);
            else if(Input.GetKeyDown(KeyCode.W))
                RPCSendCommandMoveServer(Vector2.up);
            else if(Input.GetKeyDown(KeyCode.S))
                RPCSendCommandMoveServer(Vector2.down);
            
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _entityData.CreatureSprites.SpriteAddressableKey = "warforgedmage";
                RPCSetPlayerDataServer(_entityData);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                //attack everything except ourselves
                foreach (var obj in _objectsInRange)
                {
                    var ecp = obj.GetComponent<EntityColliderPlayer>();
                    var target = ecp.GetEntity();
                    var targetID = target.UniqueID;
                    if (target != null && targetID != _entityData.UniqueID)
                        RPCSendCommandAttackServer(target.UniqueID, 10);
                }
            }
        }
    }

    void OnTick_Collision()
    {
        Logger.Log("Checking collision..", "PlayerEntity:OnTick_Collision");
        // Define the position and radius
        Vector2 position = CreatureTransform.position;
        float radius = 1f;

        // Perform the OverlapCircleAll
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        _objectsInRange = new GameObject[hits.Length];
        // Process the hits
        for (int i = 0; i < hits.Length; i++)
        {
            // Do something with each object that was hit
            Logger.Log("Hit: " + hits[i].gameObject.name, "PlayerEntity:OnTick_Collision");
            _objectsInRange[i] = hits[i].gameObject;
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

    [ServerRpc]
    public void RPCSendCommandAttackServer(string targetCreatureUniqueID, int damage)
    {
        RPCSendCommandAttackClient(targetCreatureUniqueID,damage);
    }
    
    [ObserversRpc]
    public void RPCSendCommandAttackClient(string targetCreatureUniqueID, int damage)
    {
        Logger.Log($"AttackClient {targetCreatureUniqueID}", "PlayerEntity:RPCSendCommandAttackClient");
        AttackCreature(targetCreatureUniqueID, damage);
    }
    
    [ServerRpc]
    public void RPCSendCommandMoveServer(Vector2 dir)
    {
        RPCSendCommandMoveClient(dir);
    }
    
    [ObserversRpc]
    public void RPCSendCommandMoveClient(Vector2 dir)
    {
        SetMovement(dir);
    }

    [ServerRpc]
    public void RPCEndPlayerTurnServer()
    {
        RPCEndPlayerTurnClient();   
    }

    [ObserversRpc]
    void RPCEndPlayerTurnClient()
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.ManualTick();
    }

    // [ServerRpc]
    // void RPCSpawnPlayer(NetworkConnection connection)
    // {
    //     Logger.Log($"Spawned with connection: {connection.ClientId}");
    //     var spawner = ServiceLocator.Get<IServiceNetworkSpawner>();
    //     var obj = spawner.SpawnPlayer(connection);
    // }
}