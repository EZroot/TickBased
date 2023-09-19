using System;
using FearProj.ServiceLocator;
using FishNet.Object;
using TickBased.Scripts.Commands;
using UnityEngine;
public class CreatureEntity<T> : Entity<T>, ICreatureEntity where T : CreatureEntityData
{
    [SerializeField] private EntityWearables _entityWearables;
    [SerializeField] private EntityWearables _ghostEntityWearables;
    [SerializeField] private Transform _creatureTransform;
    [SerializeField] private Transform _ghostTransform;

    public Action OnGhostEntityMovement;
    public CreatureEntityData EntityData => _entityData;
    public ITileObject TileObject => this;
    public string UniqueID => EntityData.UniqueID;
    public bool IsInitialized => EntityData != null || EntityData.UniqueID != String.Empty;

    public Transform CreatureTransform => _creatureTransform;
    public Transform GhostTransform => _ghostTransform;
    
    //Current location for updating movement commands
    private GridManager.GridCoordinate _ghostGridPosition;
    protected GameObject[] _objectsInRangeCreature;
    protected GameObject[] _objectsInRangeGhost;
    protected float _collisionRadius = 2f;

    public virtual void Start()
    {
        TickBased.Logger.Logger.Log("Start called", "CreatureEntity");

        //will probably need to sync this initial position or something
        SetGridCoordinates((int)_creatureTransform.position.x, (int) _creatureTransform.position.y);
        _ghostGridPosition = new GridManager.GridCoordinate(GridCoordinates.X,GridCoordinates.Y);
        AddCreatureToManager();
    }
    
    public override void Initialize()
    {
        TickBased.Logger.Logger.Log("Creature Entity Initialized", "CreatureEntity");
        base.Initialize();
        var lightManager = ServiceLocator.Get<IServiceLightManager>();
        lightManager.AddRadialLight(UniqueID,GridCoordinates.X,GridCoordinates.Y, 6, Color.clear);
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

    public void SetStartPosition(GridManager.GridCoordinate coordinate)
    {
        var targ = new Vector2(coordinate.X, coordinate.Y);
        CreatureTransform.position  = targ;
        TileObject.SetGridCoordinates(coordinate.X,coordinate.Y);
    }

    public void HighlightCreature()
    {
        _entityWearables.HighlightSprite();
    }

    public void UnHighlightCreature()
    {
        _entityWearables.UnHighlightSprite();
    }

    protected void SetMovement(Vector2 direction, bool useGhostPrediction = true)
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var uniqueId = EntityData.UniqueID;
        var canIssueCommand = tickManager.CheckIfCreatureCanIssueCommandThisTick(uniqueId);
        if (canIssueCommand == false)
            return;

        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        var xx = (int)(GridCoordinates.X + direction.x);
        var yy = (int)(GridCoordinates.Y + direction.y);
        SetGridCoordinates(xx, yy);
        var worldPosition = GridCoordinates * gridManager.TileSize;
        var moveCommand = new MoveCommand(this, worldPosition );

        tickManager.QueueCommand(uniqueId, moveCommand, tickManager.CurrentTick); //might need to send the current tick to make sure its synced..

        if (useGhostPrediction)
        {
            // New code for the ghost character
            var gx = (int)(_ghostGridPosition.X + direction.x);
            var gy = (int)(_ghostGridPosition.Y + direction.y);
            _ghostGridPosition = new GridManager.GridCoordinate(gx,gy);
            var ghostPos = _ghostGridPosition * gridManager.TileSize;
            UpdateGhostPosition(ghostPos);
        }
    }
    
    void UpdateGhostPosition(GridManager.GridCoordinate newPosition)
    {
        // Update the transform or the position of the ghost character based on the new grid position
        _ghostTransform.position = new Vector2(newPosition.X,newPosition.Y);
        OnGhostEntityMovement?.Invoke();
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
        _entityWearables.SetWearableAddressable(EntityWearables.WearableType.Sprite, data.CreatureSprites.SpriteAddressableKey);
        _ghostEntityWearables.SetWearableAddressable(EntityWearables.WearableType.Sprite, data.CreatureSprites.SpriteAddressableKey);
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