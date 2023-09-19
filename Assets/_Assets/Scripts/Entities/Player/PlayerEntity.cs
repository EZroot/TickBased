using FishNet.Connection;
using FishNet.Object;
using FearProj.ServiceLocator;
using TickBased.Scripts.Commands;
using UnityEngine;

public class PlayerEntity : CreatureEntity<PlayerEntityData>
{
    [SerializeField] private DisableNetworkObjects _disableNetworkObjects;

    private float _inputTimer = 0f;
    private float _inputDelay = .5f;

    #region -- Unity Methods --
    void Awake()
    {
        var sceneManager = ServiceLocator.Get<IServiceSceneManager>();
        sceneManager.OnSceneFinishedLoading += On_SceneLoad;
    }

    public override void Start()
    {
        base.Start();
        TickBased.Logger.Logger.Log("Start called", "PlayerEntity");
        var playerManager = ServiceLocator.Get<IServicePlayerManager>();

        playerManager.AddPlayer(this);

        OnEntityDataChanged += SaveEntityData;

        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.OnCommandExecuted +=
            OnCommandExecuted_CollisionUpdate; //were doin this to check collision after every movement/attack
        OnGhostEntityMovement += OnGhostEntityMovement_CollisionUpdate; //updating our ghost so we can execute commands
    }

    void Update()
    {
        if (base.IsOwner)
        {
            var tickManager = ServiceLocator.Get<IServiceTickManager>();
            if (tickManager.TickExecutionMode == TickManager.TickMode.RealTime)
            {
                _inputTimer += Time.deltaTime;

                if (_inputTimer >= _inputDelay)
                {
                    if (Input.GetKey(KeyCode.D))
                    {
                        RPCSendCommandMoveServer(Vector2.right, true);
                    }
                    else if (Input.GetKey(KeyCode.A))
                    {
                        RPCSendCommandMoveServer(Vector2.left, true);
                    }
                    else if (Input.GetKey(KeyCode.W))
                    {
                        RPCSendCommandMoveServer(Vector2.up, true);
                    }
                    else if (Input.GetKey(KeyCode.S))
                    {
                        RPCSendCommandMoveServer(Vector2.down, true);
                    }
                    _inputTimer = 0f;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.D))
                {
                    RPCSendCommandMoveServer(Vector2.right, true);
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    RPCSendCommandMoveServer(Vector2.left, true);
                }
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    RPCSendCommandMoveServer(Vector2.up, true);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    RPCSendCommandMoveServer(Vector2.down, true);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _entityData.CreatureSprites.SpriteAddressableKey = "warforgedmage";
                RPCSetPlayerDataServer(_entityData);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                //attack everything except ourselves
                foreach (var obj in _objectsInRangeGhost)
                {
                    var ecp = obj.GetComponent<EntityCollider>();
                    var target = ecp.GetEntity();
                    var targetID = target.UniqueID;
                    if (target != null && targetID != _entityData.UniqueID)
                        RPCSendCommandAttackServer(targetID, CreatureEntityData.AttackType.Bludgeon, "Head", 10);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(CreatureTransform.position, _collisionRadius);
    }

    void OnDestroy()
    {
        var playerManager = ServiceLocator.Get<IServicePlayerManager>();
        playerManager.RemovePlayer(this);
        OnEntityDataChanged -= SaveEntityData;
    }

    #endregion
    
    #region NetworkBehaviour Overrides
    public override void OnStartClient()
    {
        base.OnStartClient();
        Initialize();
        //
        // var grid = ServiceLocator.Get<IServiceGridManager>();
        // //has to be divisible by 4
        // grid.InitializeGrid(new GridManager.GridData(64,64,2));
    }
    #endregion
    
    public override void Initialize()
    {
        if (base.IsOwner)
        {
            base.Initialize();
            var netwrk = ServiceLocator.Get<IServiceNetworkManager>();
            var connection = netwrk.FishnetManager.ClientManager.Connection;

            var playerManager = ServiceLocator.Get<IServicePlayerManager>();
            playerManager.SetClientStats(connection.ClientId, playerManager.ClientStats.Username);

            var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
            creatureManager.SetPlayer(this);
            
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
                TickBased.Logger.Logger.Log("On Scene Load - Enabling Player Body", "PlayerEntity");
                RPCSetPlayerBodyEnabledServer(true);
            }
            else
            {
                TickBased.Logger.Logger.Log("On Scene Load - Disabling Player Body", "PlayerEntity");
                RPCSetPlayerBodyEnabledServer(false);
            }
        }
    }

    protected override void OnGhostEntityMovement_CollisionUpdate()
    {
        base.OnGhostEntityMovement_CollisionUpdate();

        if (base.IsOwner)
        {

            var uiManager = ServiceLocator.Get<IServiceUIManager>();

            //clear interaction choices
            uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();

            //creature interaction choices
            foreach (var creatures in _objectsInRangeGhost)
            {
                var collider = creatures.GetComponent<EntityCollider>();
                var creature = collider.GetEntity();
                uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this,
                    creature,
                    $"Interact {creature.EntityData.CreatureStats.Name}",
                    () =>
                    {
                        //we can add the UI changes here
                        //Select entity -> Select Action (Wrestle/etc/) -> Selection specifics -> Roll chance, do attack success/fail
                        //RPCSendCommandAttackServer(creature.UniqueID, CreatureEntityData.AttackType.Bludgeon, "Head", 10);
                        Interaction_SpawnInteractionChoices(creature);
                    });
            }
        }
    }
    
    #region -- Creature Collision Interactions --
    void Interaction_SpawnInteractionChoices(ICreatureEntity interactionTarget)
    {
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Attack {interactionTarget.EntityData.CreatureStats.Name}",
            ()=>
            {
                Interaction_SpawnSubActions_Attack(CreatureEntityData.AttackType.Bludgeon, interactionTarget);
            });
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Wrestle {interactionTarget.EntityData.CreatureStats.Name}",
            ()=>
            {
                Interaction_SpawnSubActions_Wrestle(CreatureEntityData.WrestleType.Grab, interactionTarget);
            });
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Examine {interactionTarget.EntityData.CreatureStats.Name}",
            ()=>
            {
                TickBased.Logger.Logger.Log("Youre too dumb to examine this creature");
            });
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Speak {interactionTarget.EntityData.CreatureStats.Name}",
            ()=>
            {
                TickBased.Logger.Logger.Log("The creature is not interested in speaking with you");
            });
    }

    void Interaction_SpawnSubActions_Attack(CreatureEntityData.AttackType attackType, ICreatureEntity attackTarget)
    {
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();

        foreach (var limb in attackTarget.EntityData.CreatureStats.CreatureLimbs)
        {
            uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, attackTarget,
                $"{attackType.ToString()} {limb.LimbName} of {attackTarget.EntityData.CreatureStats.Name}",
                () =>
                {
                    RPCSendCommandAttackServer(attackTarget.UniqueID, attackType, limb.LimbName, 10); });
        }
    }
    
    void Interaction_SpawnSubActions_Wrestle(CreatureEntityData.WrestleType wrestleType, ICreatureEntity attackTarget)
    {
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();

        foreach (var limb in attackTarget.EntityData.CreatureStats.CreatureLimbs)
        {
            uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, attackTarget,
                $"{wrestleType.ToString()} {limb.LimbName} of {attackTarget.EntityData.CreatureStats.Name}",
                () =>
                {
                    RPCSendCommandWrestleServer(attackTarget.UniqueID, wrestleType, limb.LimbName, 10); });
        }
    }
    #endregion
    
    #region -- RPC List --
    [ServerRpc]
    void RPCSetPlayerDataServer(PlayerEntityData data)
    {
        RPCSetPlayerDataClient(data);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetPlayerDataClient(PlayerEntityData data)
    {
        TickBased.Logger.Logger.Log("Setting Entity Data", "RPCSetPlayerDataClient");
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
    
    [ServerRpc(RequireOwnership = false)]
    public void RPCSendCommandWaitServer()
    {
        RPCSendCommandWaitClient();
    }

    [ObserversRpc]
    public void RPCSendCommandWaitClient()
    {
        var tick = ServiceLocator.Get<IServiceTickManager>();
        tick.QueueCommand(_entityData.UniqueID, new WaitCommand(), tick.CurrentTick);
    }

    [ServerRpc]
    public void RPCSendCommandMoveServer(Vector2 dir, bool useGhostPrediction)
    {
        RPCSendCommandMoveClient(dir,useGhostPrediction);
    }

    [ObserversRpc]
    public void RPCSendCommandMoveClient(Vector2 dir,bool useGhostPrediction)
    {
        SetMovement(dir, useGhostPrediction);
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
    
    [ServerRpc]
    public void RPCSwitchTickModeServer()
    {
        RPCSwitchTickModeClient();
    }

    [ObserversRpc]
    void RPCSwitchTickModeClient()
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var tickModeManual = tickManager.TickExecutionMode == TickManager.TickMode.Manual;
        if(tickModeManual)
            tickManager.TickExecutionMode = TickManager.TickMode.RealTime;
        else
            tickManager.TickExecutionMode = TickManager.TickMode.Manual;
    }
    
    #endregion
}