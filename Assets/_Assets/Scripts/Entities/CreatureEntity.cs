using System;
using FearProj.ServiceLocator;
using FishNet.Object;
using TickBased.Scripts.Commands;
using UnityEngine;
public class CreatureEntity<T> : Entity<T>, ICreatureEntity where T : CreatureEntityData
{
    [SerializeField] private int _entityVision = 8;
    [SerializeField] protected float _collisionRadius = 2f;
    [SerializeField] private EntityWearables _entityWearables;
    [SerializeField] private EntityWearables _ghostEntityWearables;
    [SerializeField] private Transform _creatureTransform;
    [SerializeField] private Transform _ghostTransform;
    [SerializeField] private GridManager.TileState _occupiedTileState = GridManager.TileState.Object;
    [SerializeField] private CreatureInventory _creatureInventory;
    [Header("What will appear when the creature dies")]
    [SerializeField] private GameObject _deathEffect;

    protected bool _canInteract = true;
    
    public Action OnDeath;
    public Action OnGhostEntityMovement;
    public virtual bool CanInteract => _canInteract;
    public CreatureEntityData EntityData => _entityData;
    public CreatureInventory CreatureInventory => _creatureInventory;
    public ITileObject TileObject => this;
    public string UniqueID => EntityData.UniqueID;
    public bool IsInitialized => EntityData != null || EntityData.UniqueID != String.Empty;

    public Transform Transform => transform;
    public Transform CreatureTransform => _creatureTransform;
    public Transform GhostTransform => _ghostTransform;
    
    //Current location for updating movement commands
    [Header("- DEBUG - dont change this")]
    [SerializeField] private GridManager.GridCoordinate _ghostGridPosition;
    [SerializeField]protected GameObject[] _objectsInRangeCreature;
    [SerializeField]protected GameObject[] _objectsInRangeGhost;

    public virtual void Start()
    {
        TickBased.Logger.Logger.Log("Start called", "CreatureEntity");
        _creatureInventory = new CreatureInventory();
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
            tickManager.OnCommandExecuted += OnSavedData_CheckIfDead;
        //will probably need to sync this initial position or something
        SetGridCoordinates((int)_creatureTransform.transform.position.x, (int) _creatureTransform.transform.position.y, _occupiedTileState);
        _creatureTransform.transform.localPosition = transform.position;
        _ghostGridPosition = new GridManager.GridCoordinate(GridCoordinates.X,GridCoordinates.Y);
        AddCreatureToManager();
    }

    void OnSavedData_CheckIfDead()
    {
        TickBased.Logger.Logger.Log("Checking if dead", "Dead");
        foreach (var limb in EntityData.CreatureStats.CreatureLimbs)
        {
            TickBased.Logger.Logger.Log($"Checking limb [{limb.LimbName} {limb.Health}]" , "Dead");

            if (limb.Health <= 0)
            {
                TickBased.Logger.Logger.Log($"setting is dead {EntityData.IsDead} [{limb.LimbName} {limb.Health}]" , "Dead");

                EntityData.IsDead = true;
                TickBased.Logger.Logger.Log($"finished setting is dead {EntityData.IsDead} [{limb.LimbName} {limb.Health}]" , "Dead");

                _deathEffect.SetActive(true);
                
                OnDeath?.Invoke();
                break;
            }
        }
    }
    
    public override void Initialize()
    {
        TickBased.Logger.Logger.Log("Creature Entity Initialized", "CreatureEntity");
        base.Initialize();
        var lightManager = ServiceLocator.Get<IServiceLightManager>();
        lightManager.AddRadialLight(UniqueID,GridCoordinates.X,GridCoordinates.Y, _entityVision, Color.clear);
    }

    public override void Initialize(string uniqueId)
    {
        base.Initialize(uniqueId);
        GhostTransform.transform.localPosition = CreatureTransform.transform.localPosition;
    }

    protected void AddCreatureToManager()
    {
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        creatureManager.AddCreature(this);
    }
    


    public void SetDirection(Quaternion rotation)
    {
        _creatureTransform.rotation = rotation;
    }

    public void SetStartPosition(GridManager.GridCoordinate coordinate, GridManager.TileState tileState)
    {
        var targ = new Vector2(coordinate.X, coordinate.Y);
        var mgr = ServiceLocator.Get<IServiceGridManager>();
        var gridTarg = mgr.GridToWorld(coordinate);
        
        //_gridCoordinates = coordinate;
        CreatureTransform.localPosition  = gridTarg;
        GhostTransform.transform.localPosition = CreatureTransform.transform.localPosition;
       TileObject.SetGridCoordinates(coordinate.X,coordinate.Y, tileState);
       var camPos = Camera.allCameras[0].transform.position;
       Camera.allCameras[0].transform.position = new Vector3(CreatureTransform.localPosition.x, CreatureTransform.localPosition.y,camPos.z);
    }

    public void HighlightCreature()
    {
        _entityWearables.HighlightSprite();
    }

    public void UnHighlightCreature()
    {
        _entityWearables.UnHighlightSprite();
    }

    protected void SetMovement(GridManager.GridCoordinate newPosition, bool useGhostPrediction = true)
    {
        if (EntityData.IsDead)
            return;
        
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var uniqueId = EntityData.UniqueID;
        var canIssueCommand = tickManager.CheckIfCreatureCanIssueCommandThisTick(uniqueId);
        if (canIssueCommand == false)
            return;

        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        // newPosition = new GridManager.GridCoordinate(newPosition.X,
        //     newPosition.Y);
        var xx = (int)(newPosition.X);
        var yy = (int)(newPosition.Y);
        SetGridCoordinates(xx, yy, _occupiedTileState);
        var wpg = gridManager.GridToWorld(GridCoordinates);
        var worldPosition = new GridManager.GridCoordinate((int)wpg.x,(int)wpg.y);
        var moveCommand = new MoveCommand(this, worldPosition );

        tickManager.QueueCommand(uniqueId, moveCommand, tickManager.CurrentTick); //might need to send the current tick to make sure its synced..

        if (useGhostPrediction)
        {
            // New code for the ghost character
            var gx = (int)(newPosition.X);
            var gy = (int)(newPosition.Y);
            _ghostGridPosition = new GridManager.GridCoordinate(gx,gy);
            var ghostPos = worldPosition;
            UpdateGhostPosition(ghostPos);
        }
    }
    
    void UpdateGhostPosition(GridManager.GridCoordinate newPosition)
    {
        // Update the transform or the position of the ghost character based on the new grid position
        _ghostTransform.position = new Vector2(newPosition.X,newPosition.Y);
        OnGhostEntityMovement?.Invoke();
    }
    
    protected void BuildObject(string targetCreatureUniqueID, int amountOfWorkToAdd)
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var uniqueId = EntityData.UniqueID;
        var canIssueCommand = tickManager.CheckIfCreatureCanIssueCommandThisTick(uniqueId);
        if (canIssueCommand == false)
            return;
        
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        var creatureData = creatureManager.GetCreature(targetCreatureUniqueID) as IBuildableObject;
        var attackCommand = new BuildCommand(this, creatureData, amountOfWorkToAdd);
        tickManager.QueueCommand(uniqueId, attackCommand, tickManager.CurrentTick);
    }

    protected void PickupItem(string targetCreatureUniqueID)
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var uniqueId = EntityData.UniqueID;
        var canIssueCommand = tickManager.CheckIfCreatureCanIssueCommandThisTick(uniqueId);
        if (canIssueCommand == false)
            return;
        
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        var creatureData = creatureManager.GetCreature(targetCreatureUniqueID) as IInventoryItem;
        var attackCommand = new PickupCommand(this, creatureData);
        tickManager.QueueCommand(uniqueId, attackCommand, tickManager.CurrentTick);
    }
    
    protected void AttackCreature(string targetCreatureUniqueID, CreatureEntityData.AttackType attackType, string limbName, int damage)
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var uniqueId = EntityData.UniqueID;
        var canIssueCommand = tickManager.CheckIfCreatureCanIssueCommandThisTick(uniqueId);
        if (canIssueCommand == false)
            return;
        
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        var creatureData = creatureManager.GetCreature(targetCreatureUniqueID);
        var attackCommand = new AttackCommand(this, creatureData, attackType, damage, limbName);
        tickManager.QueueCommand(uniqueId, attackCommand, tickManager.CurrentTick);
    }
    
    protected void WrestleCreature(string targetCreatureUniqueID, CreatureEntityData.WrestleType wrestleType, string limbName, int damage)
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var uniqueId = EntityData.UniqueID;
        var canIssueCommand = tickManager.CheckIfCreatureCanIssueCommandThisTick(uniqueId);
        if (canIssueCommand == false)
            return;
        
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        var creatureData = creatureManager.GetCreature(targetCreatureUniqueID);
        var wrestleCommand = new WrestleCommand(this, creatureData, wrestleType, damage, limbName);
        tickManager.QueueCommand(uniqueId, wrestleCommand, tickManager.CurrentTick);
    }
    
    public override void SetEntityData(T data)
    {
        base.SetEntityData(data);
        UpdateSpriteAddressables(data);
    }

    protected void UpdateSpriteAddressables(CreatureEntityData data)
    {
        TickBased.Logger.Logger.Log("Loading wearable addressables", "CreatureEntity");
        if (_entityWearables.isActiveAndEnabled )
        {
            _entityWearables.SetWearableAddressable(EntityWearables.WearableType.Sprite,
                data.CreatureSprites.SpriteAddressableKey);
            _ghostEntityWearables.SetWearableAddressable(EntityWearables.WearableType.Sprite,
                data.CreatureSprites.SpriteAddressableKey);
        }
    }
    
    protected virtual void OnCommandExecuted_CollisionUpdate()
    {
        // Define the position and radius
        Vector2 position = CreatureTransform.position;

        // Perform the OverlapCircleAll
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, _collisionRadius);
        _objectsInRangeCreature = new GameObject[hits.Length];
        // Process the hits
        for (int i = 0; i < hits.Length; i++)
        {
            // Do something with each object that was hit
            TickBased.Logger.Logger.Log("Hit: " + hits[i].gameObject.name, "PlayerEntity:OnTick_Collision");
            _objectsInRangeCreature[i] = hits[i].gameObject;
        }
    }
    
    protected virtual void OnGhostEntityMovement_CollisionUpdate()
    {
        // Define the position and radius
        Vector2 position = GhostTransform.position;

        // Perform the OverlapCircleAll
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, _collisionRadius);
        _objectsInRangeGhost = new GameObject[hits.Length];
        // Process the hits
        for (int i = 0; i < hits.Length; i++)
        {
            // Do something with each object that was hit
            TickBased.Logger.Logger.Log("Hit: " + hits[i].gameObject.name, "PlayerEntity:OnTick_Collision");
            _objectsInRangeGhost[i] = hits[i].gameObject;
        }
    }

    public void UpdateWearableAlpha(float alphaLevel)
    {
        _entityWearables.SetSpriteAlpha(alphaLevel);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RPCSendCommandBuildServer(string targetItemUniqueID, int workToAdd)
    {
        RPCSendCommandBuildClient(targetItemUniqueID,workToAdd);
    }
    
    [ObserversRpc]
    public void RPCSendCommandBuildClient(string targetItemUniqueID, int workToAdd)
    {
        BuildObject(targetItemUniqueID,workToAdd);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RPCSendCommandPickupServer(string targetItemUniqueID)
    {
        RPCSendCommandPickupClient(targetItemUniqueID);
    }
    
    [ObserversRpc]
    public void RPCSendCommandPickupClient(string targetItemUniqueID)
    {
        PickupItem(targetItemUniqueID);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RPCSendCommandAttackServer(string targetCreatureUniqueID, CreatureEntityData.AttackType attackType, string limbName, int damage)
    {
        RPCSendCommandAttackClient(targetCreatureUniqueID,attackType,limbName,damage);
    }
    
    [ObserversRpc]
    public void RPCSendCommandAttackClient(string targetCreatureUniqueID, CreatureEntityData.AttackType attackType, string limbName, int damage)
    {
        TickBased.Logger.Logger.Log($"AttackClient {targetCreatureUniqueID}", "PlayerEntity:RPCSendCommandAttackClient");
        AttackCreature(targetCreatureUniqueID,attackType,limbName,damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RPCSendCommandWrestleServer(string targetCreatureUniqueID, CreatureEntityData.WrestleType wrestleType, string limbName, int damage)
    {
        RPCSendCommandWrestleClient(targetCreatureUniqueID,wrestleType,limbName,damage);
    }
    
    [ObserversRpc]
    public void RPCSendCommandWrestleClient(string targetCreatureUniqueID, CreatureEntityData.WrestleType wrestleType, string limbName, int damage)
    {
        TickBased.Logger.Logger.Log($"WrestleClient {targetCreatureUniqueID}", "PlayerEntity:RPCSendCommandWrestleClient");
        WrestleCreature(targetCreatureUniqueID,wrestleType,limbName,damage);
    }
}