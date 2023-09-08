using FearProj.ServiceLocator;
using TickBased.Scripts.Commands;
using UnityEngine;
public class CreatureEntity<T> : Entity<T> where T : CreatureEntityData
{
    [SerializeField] private EntityWearables _entityWearables;
    [SerializeField] private Transform _creatureTransform;

    //Current location for updating movement commands
    private Vector2 _currentGridPosition;
    
    public override void Initialize()
    {
        Logger.Log("Creature Entity Initialized", "CreatureEntity");
        base.Initialize();
        _currentGridPosition = _creatureTransform.position;
    }

    protected void SetDirection(Quaternion rotation)
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