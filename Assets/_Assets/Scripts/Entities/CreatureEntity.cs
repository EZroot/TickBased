using System;
using FearProj.ServiceLocator;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
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
    public string UniqueID => EntityData.UniqueID;
    public bool IsInitialized => EntityData != null || EntityData.UniqueID != String.Empty;

    public Transform CreatureTransform => _creatureTransform;
    public Transform GhostTransform => _ghostTransform;
    
    //Current location for updating movement commands
    private Vector2 _currentGridPosition;
    private Vector2 _ghostGridPosition;
    protected GameObject[] _objectsInRangeCreature;
    protected GameObject[] _objectsInRangeGhost;
    protected float _collisionRadius = 2f;

    public virtual void Start()
    {
        Logger.Log("Start called", "CreatureEntity");

        //will probably need to sync this initial position or something
        _currentGridPosition = _creatureTransform.position;
        _ghostGridPosition = _currentGridPosition;
        AddCreatureToManager();
    }
    
    public override void Initialize()
    {
        Logger.Log("Creature Entity Initialized", "CreatureEntity");
        base.Initialize();
        GenerateUniqueID();
    }

    protected void AddCreatureToManager()
    {
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        creatureManager.AddCreature(this);
    }
    
    protected void GenerateUniqueID()
    {
        var id = Guid.NewGuid().ToString();
        Logger.Log($"Generating unique ID {id}", "CreatureEntity");
        EntityData.UniqueID = id;
    }

    public void SetDirection(Quaternion rotation)
    {
        _creatureTransform.rotation = rotation;
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
        
        _currentGridPosition += direction;
        var moveCommand = new MoveCommand(_creatureTransform, _currentGridPosition);

        tickManager.QueueCommand(uniqueId, moveCommand, tickManager.CurrentTick); //might need to send the current tick to make sure its synced..

        if (useGhostPrediction)
        {
            // New code for the ghost character
            _ghostGridPosition += direction;
            UpdateGhostPosition(_ghostGridPosition);
        }
    }
    
    void UpdateGhostPosition(Vector2 newPosition)
    {
        // Update the transform or the position of the ghost character based on the new grid position
        _ghostTransform.position = _ghostGridPosition;
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
        Logger.Log("Loading wearable addressables", "CreatureEntity");
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
            Logger.Log("Hit: " + hits[i].gameObject.name, "PlayerEntity:OnTick_Collision");
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
            Logger.Log("Hit: " + hits[i].gameObject.name, "PlayerEntity:OnTick_Collision");
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
        Logger.Log($"AttackClient {targetCreatureUniqueID}", "PlayerEntity:RPCSendCommandAttackClient");
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
        Logger.Log($"WrestleClient {targetCreatureUniqueID}", "PlayerEntity:RPCSendCommandWrestleClient");
        WrestleCreature(targetCreatureUniqueID,wrestleType,limbName,damage);
    }
}