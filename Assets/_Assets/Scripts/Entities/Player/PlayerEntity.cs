using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FearProj.ServiceLocator;
using TickBased.Scripts.Commands;
using UnityEngine;
using Logger = TickBased.Logger.Logger;
using Random = UnityEngine.Random;

public class PlayerEntity : CreatureEntity<PlayerEntityData>
{
    [SerializeField] private DisableNetworkObjects _disableNetworkObjects;
    private PlayerMobileInput _playerMobileInput;
    private float _inputTimer = 0f;
    private float _inputDelay = .25f;
    #region -- Unity Methods --
    void Awake()
    {
        var sceneManager = ServiceLocator.Get<IServiceSceneManager>();
        sceneManager.OnSceneFinishedLoading += On_SceneLoad;
    }
    private void OnGUI()
    {
        if (IsOwner == false)
            return;
        var xPos = Screen.width - 250;
        GUI.Box(new Rect(xPos,0,205,160), "Player");
        GUI.Label(new Rect(xPos, 15, 200, 20), $"Character: [{GridCoordinates.X},{GridCoordinates.Y}]");
        GUI.Label(new Rect(xPos, 35, 200, 20), $"Ghost: [{_ghostGridPosition.X},{_ghostGridPosition.Y}]");
        GUI.Label(new Rect(xPos, 55, 200, 20), $"ID: {EntityData.UniqueID}");
        GUI.Label(new Rect(xPos, 75, 200, 20), $"User: {EntityData.ClientStats.Username}");
        GUI.Label(new Rect(xPos, 95, 200, 20), $"Vision: {EntityVision}");
        GUI.Label(new Rect(xPos, 115, 200, 20), $"Faction: {EntityData.CreatureStats.Faction.FactionType}");
    }
    public override void Start()
    {
        base.Start();
        TickBased.Logger.Logger.Log("Start called", "PlayerEntity");
        var playerManager = ServiceLocator.Get<IServicePlayerManager>();
        playerManager.AddPlayer(this);

        OnEntityDataChanged += SaveEntityData;
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.PostTick +=
            OnCommandExecuted_CollisionUpdate; //were doin this to check collision after every movement/attack
        //
#if UNITY_ANDROID && !UNITY_EDITOR
        _playerMobileInput = new PlayerMobileInput();
#endif
    }

    void Update()
    {
        if (base.IsOwner)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                _playerMobileInput.UpdateInputDetection((PlayerMobileInput.SwipDirection swipeDirection) =>
                {
                    var newPos = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y);
                    switch (swipeDirection)
                    {
                        case PlayerMobileInput.SwipDirection.Up:
                             newPos = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y+ 1);
                            RPCSendCommandMoveServer(newPos, true);
                            break;
                        case PlayerMobileInput.SwipDirection.Down:
                             newPos = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y- 1);
                            RPCSendCommandMoveServer(newPos, true);
                            break;
                        case PlayerMobileInput.SwipDirection.Right:
                             newPos = new GridManager.GridCoordinate(GridCoordinates.X + 1, GridCoordinates.Y);
                            RPCSendCommandMoveServer(newPos, true);
                            break;
                        case PlayerMobileInput.SwipDirection.Left:
                             newPos = new GridManager.GridCoordinate(GridCoordinates.X - 1, GridCoordinates.Y);
                            RPCSendCommandMoveServer(newPos, true);
                            break;
                    }
                });
            #else
            var tickManager = ServiceLocator.Get<IServiceTickManager>();
            if (tickManager.TickExecutionMode == TickManager.TickMode.RealTime && tickManager.IsExecutingTick == false)
            {
                _inputTimer += Time.deltaTime;
                
                //if (_inputTimer >= _inputDelay && tickManager.IsExecutingTick == false)
                //{
                    var gridManager = ServiceLocator.Get<IServiceGridManager>();
                    if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.W))
                    {
                        
                        var newPos = new GridManager.GridCoordinate(GridCoordinates.X + 1, GridCoordinates.Y + 1);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }
                    else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.S))
                    {
                        
                        var newPos = new GridManager.GridCoordinate(GridCoordinates.X + 1, GridCoordinates.Y - 1);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }
                    else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.W))
                    {
                               
                 var newPos = new GridManager.GridCoordinate(GridCoordinates.X - 1, GridCoordinates.Y + 1);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }
                    else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S))
                    {
                                 
               var newPos = new GridManager.GridCoordinate(GridCoordinates.X - 1, GridCoordinates.Y - 1);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }
                    
                    else if (Input.GetKey(KeyCode.D))
                    {
                                     
           var newPos = new GridManager.GridCoordinate(GridCoordinates.X + 1, GridCoordinates.Y);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }
                    else if (Input.GetKey(KeyCode.A))
                    {
                                        
        var newPos = new GridManager.GridCoordinate(GridCoordinates.X - 1, GridCoordinates.Y);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }
                    else if (Input.GetKey(KeyCode.W))
                    {
                                        
        var newPos = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y + 1);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }
                    else if (Input.GetKey(KeyCode.S))
                    {
                                        
        var newPos = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y - 1);
                        var tile = gridManager.GetTile(newPos.X, newPos.Y);
                        if(tile.State == GridManager.TileState.Empty)
                            RPCSendCommandMoveServer(newPos, true);
                    }

                 //   _inputTimer = 0f;
                //}
            }
            else if (tickManager.IsExecutingTick == false)
            {
                if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.W))
                {
                                          
  var newPos = new GridManager.GridCoordinate(GridCoordinates.X + 1, GridCoordinates.Y + 1);
                    RPCSendCommandMoveServer(newPos, true);
                }
                else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.S))
                {
                                          
  var newPos = new GridManager.GridCoordinate(GridCoordinates.X + 1, GridCoordinates.Y - 1);
                    RPCSendCommandMoveServer(newPos, true);
                }
                else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.W))
                {
                                          
  var newPos = new GridManager.GridCoordinate(GridCoordinates.X - 1, GridCoordinates.Y + 1);
                    RPCSendCommandMoveServer(newPos, true);
                }
                else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S))
                {
                                          
  var newPos = new GridManager.GridCoordinate(GridCoordinates.X - 1, GridCoordinates.Y - 1);
                    RPCSendCommandMoveServer(newPos, true);
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                                         
   var newPos = new GridManager.GridCoordinate(GridCoordinates.X + 1, GridCoordinates.Y);
                    RPCSendCommandMoveServer(newPos, true);
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                                          
  var newPos = new GridManager.GridCoordinate(GridCoordinates.X - 1, GridCoordinates.Y);
                    RPCSendCommandMoveServer(newPos, true);
                }
                else if (Input.GetKeyDown(KeyCode.W))
                {
                                         
   var newPos = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y + 1);
                    RPCSendCommandMoveServer(newPos, true);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                                         
   var newPos = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y - 1);
                    RPCSendCommandMoveServer(newPos, true);
                }
            }
            #endif

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _entityData.CreatureSprites.SpriteAddressableKey = "warforgedmage";
                RPCSetPlayerDataServer(_entityData);
            }

            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     //attack everything except ourselves
            //     foreach (var obj in _objectsInRangeGhost)
            //     {
            //         var ecp = obj.GetComponent<EntityCollider>();
            //         var target = ecp.GetEntity();
            //         var targetID = target.UniqueID;
            //         if (target != null && targetID != _entityData.UniqueID)
            //             RPCSendCommandAttackServer(targetID, CreatureEntityData.AttackType.Bludgeon, "Head", 10);
            //     }
            // }
        }
    }

    private List<GridManager.GridCoordinate> debugPathCoords;
    private void FixedUpdate()
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        if (tickManager.IsExecutingTick)
            return;
        
        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        var pathFinder = gridManager.PathFinder;
        var pos = gridManager.GetGridPositionFromMouse(Camera.allCameras[0]);

        if (pos.X > 0 && pos.Y > 0 && pos.X < gridManager.Grid.Width && pos.Y < gridManager.Grid.Height)
        {
            if (pos.X != GridCoordinates.X || pos.Y != GridCoordinates.Y)
            {
                GridManager.GridCoordinate start = new GridManager.GridCoordinate(GridCoordinates.X, GridCoordinates.Y);
                GridManager.GridCoordinate end = pos;
                
//                Logger.Log($"Mouse Pos: {end.X} {end.Y} Grid: {GridCoordinates.X} {GridCoordinates.Y}","PlayerEntity");
                if (pathFinder != null)
                    debugPathCoords = pathFinder.FindPathImmediately(start, end);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!IsOwner)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(CreatureTransform.position, _collisionRadius);
        
        if (debugPathCoords != null)
        {
            Gizmos.color = Color.red;
            foreach (GridManager.GridCoordinate coord in debugPathCoords)
            {
                var gridmgr = ServiceLocator.Get<IServiceGridManager>();
                Vector3 position = gridmgr.GridToWorld(coord); //new Vector3(coord.X, coord.Y, 0);  // Assuming Y is up
                Gizmos.DrawCube(position, Vector3.one);
            }
        }
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

            var gridManager = ServiceLocator.Get<IServiceGridManager>();
            gridManager.OnPostGridGeneration += () =>
            {
                SetStartPosition(new GridManager.GridCoordinate(24, 24), GridManager.TileState.Object);
            };

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
        if(!IsHost)
            _disableNetworkObjects.DisableScriptsIfNotHost(false);
        
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

        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        if (base.IsOwner)
        {
            var uiManager = ServiceLocator.Get<IServiceUIManager>();

            //clear interaction choices
            uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();

            //creature interaction choices
            foreach (var creatures in _objectsInRangeGhost)
            {
                var creature = creatures as ICreatureEntity;
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
    
    protected override void OnCommandExecuted_CollisionUpdate()
    {
        base.OnCommandExecuted_CollisionUpdate();
        var tickManager = ServiceLocator.Get<IServiceTickManager>();

        if (base.IsOwner && tickManager.TickExecutionMode == TickManager.TickMode.RealTime)
        {
            var uiManager = ServiceLocator.Get<IServiceUIManager>();

            //clear interaction choices
            uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();

            //creature interaction choices
            foreach (var creatures in _objectsInRangeCreature)
            {
                var creature = creatures as ICreatureEntity;
                if (creature.CanInteract)
                {
                    uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this,
                        creature,
                        $"Interact {creature.EntityData.CreatureStats.Name}",
                        () => { Interaction_SpawnInteractionChoices(creature); });
                }
            }
            
        }
    }

    #region -- Creature Collision Interactions --

    void Interaction_SpawnInteractionChoices(ICreatureEntity interactionTarget)
    {

        if (interactionTarget is IInventoryItem) //has to be first
            Interaction_SetupInventoryItemOptions(interactionTarget);
        else if (interactionTarget is IBuildableObject) //has to be first
            Interaction_SetupInventoryBuildableOptions(interactionTarget);
        else //assume its a creature, cuz it is
            Interaction_SetupAttackOptions(interactionTarget);

    }
    
    void Interaction_SetupInventoryItemOptions(ICreatureEntity interactionTarget)
    {
        var inventoryItem = interactionTarget as IInventoryItem;
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Pickup {interactionTarget.EntityData.CreatureStats.Name}",
            () => { RPCSendCommandPickupServer(interactionTarget.UniqueID); }); //this should be an RPC command cuz its an action
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Examine {interactionTarget.EntityData.CreatureStats.Name}",
            () => { TickBased.Logger.Logger.Log("Youre too dumb to examine this creature"); });
    }
    
    void Interaction_SetupInventoryBuildableOptions(ICreatureEntity interactionTarget)
    {
        var buildable = interactionTarget as IBuildableObject;
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();
        if (buildable.IsConstructionCompleted == false)
        {
            uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
                $"Build {interactionTarget.EntityData.CreatureStats.Name} {buildable.CurrentWork}/{buildable.WorkRequired}",
                () =>
                {
                    RPCSendCommandBuildServer(interactionTarget.UniqueID, 1);
                }); //this should be an RPC command cuz its an action
        }
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Examine {interactionTarget.EntityData.CreatureStats.Name}",
            () => { TickBased.Logger.Logger.Log("Youre too dumb to examine this"); });
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Destroy {interactionTarget.EntityData.CreatureStats.Name}",
            () => { TickBased.Logger.Logger.Log("You cant break this yet"); });
    }

    void Interaction_SetupAttackOptions(ICreatureEntity interactionTarget)
    {
        if (interactionTarget.EntityData.IsDead)
            return;
        
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Attack {interactionTarget.EntityData.CreatureStats.Name}",
            () => { Interaction_SpawnSubActions_Attack(CreatureEntityData.AttackType.Bludgeon, interactionTarget); });
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Wrestle {interactionTarget.EntityData.CreatureStats.Name}",
            () => { Interaction_SpawnSubActions_Wrestle(CreatureEntityData.WrestleType.Grab, interactionTarget); });
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Examine {interactionTarget.EntityData.CreatureStats.Name}",
            () => { TickBased.Logger.Logger.Log("Youre too dumb to examine this creature"); });
        uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, interactionTarget,
            $"Speak {interactionTarget.EntityData.CreatureStats.Name}",
            () => { TickBased.Logger.Logger.Log("The creature is not interested in speaking with you"); });
    }

    void Interaction_SpawnSubActions_Attack(CreatureEntityData.AttackType attackType, ICreatureEntity attackTarget)
    {
        var uiManager = ServiceLocator.Get<IServiceUIManager>();
        uiManager.UIInteractionChoiceManager.RemoveAllInteractionChoices();

        foreach (var limb in attackTarget.EntityData.CreatureStats.CreatureLimbs)
        {
            uiManager.UIInteractionChoiceManager.SpawnInteractionChoice(this, attackTarget,
                $"{attackType.ToString()} {limb.LimbName} of {attackTarget.EntityData.CreatureStats.Name}",
                () => { RPCSendCommandAttackServer(attackTarget.UniqueID, attackType, limb.LimbName, Random.Range(25,125)); });
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
                () => { RPCSendCommandWrestleServer(attackTarget.UniqueID, wrestleType, limb.LimbName, 10); });
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
    public void RPCSendCommandMoveServer(GridManager.GridCoordinate newPos, bool useGhostPrediction)
    {
        RPCSendCommandMoveClient(newPos, useGhostPrediction);
    }

    [ObserversRpc]
    public void RPCSendCommandMoveClient(GridManager.GridCoordinate newPos, bool useGhostPrediction)
    {
        SetMovement(newPos, useGhostPrediction);
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
        if (tickModeManual)
            tickManager.TickExecutionMode = TickManager.TickMode.RealTime;
        else
            tickManager.TickExecutionMode = TickManager.TickMode.Manual;
    }
    
    [ServerRpc]
    public void RPCSwitchTickModeWaitForPlayerServer()
    {
        RPCSwitchTickModeWaitForPlayerClient();
    }

    [ObserversRpc]
    void RPCSwitchTickModeWaitForPlayerClient()
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var isForcePlayerActions = tickManager.TickModeRealTime == TickManager.TickModeRealTimeType.ForcePlayerAction;
        if (isForcePlayerActions)
            tickManager.TickModeRealTime = TickManager.TickModeRealTimeType.WaitForAllPlayers;
        else
            tickManager.TickModeRealTime = TickManager.TickModeRealTimeType.ForcePlayerAction;
    }
    
    [ServerRpc]
    public void RPCSwitchTickModeCommandExecutionModeServer()
    {
        RPCSwitchTickModeCommandExecutionModeClient();
    }

    [ObserversRpc]
    void RPCSwitchTickModeCommandExecutionModeClient()
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var isConcurrent = tickManager.CommandExecutionType == TickManager.CommandExecutionMode.Concurrently;
        if (isConcurrent)
            tickManager.CommandExecutionType = TickManager.CommandExecutionMode.Sequentially;
        else
            tickManager.CommandExecutionType = TickManager.CommandExecutionMode.Concurrently;
    }

    #endregion
}