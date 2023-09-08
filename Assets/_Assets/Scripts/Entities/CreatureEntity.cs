using System;
using FearProj.ServiceLocator;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TickBased.Scripts.Commands;
using UnityEngine;
public class CreatureEntity<T> : Entity<T>, ICreatureEntity where T : CreatureEntityData
{
    [SerializeField] private EntityWearables _entityWearables;
    [SerializeField] private Transform _creatureTransform; 
    public CreatureEntityData EntityData => _entityData;
    public string UniqueID => EntityData.UniqueID;

    public Transform CreatureTransform => _creatureTransform;
    
    //Current location for updating movement commands
    private Vector2 _currentGridPosition;

    public virtual void Start()
    {
        Logger.Log("Start called", "CreatureEntity");

        _currentGridPosition = _creatureTransform.position;

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

    protected void SetMovement(Vector2 direction)
    {
        _currentGridPosition += direction;
        var moveCommand = new MoveCommand(_creatureTransform, _currentGridPosition);

        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.QueueCommand(moveCommand);
    }

    protected void AttackCreature(string targetCreatureUniqueID, int damage)
    {
        Logger.Log($"Trying to attack {targetCreatureUniqueID}", "CreatureEntity");
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        var creatureData = creatureManager.GetCreature(targetCreatureUniqueID);
        var attackCommand = new AttackCommand(this, creatureData, damage);
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.QueueCommand(attackCommand);
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
    }
}