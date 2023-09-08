using FearProj.ServiceLocator;
using UnityEngine;
public class CreatureEntity<T> : Entity<T> where T : CreatureEntityData
{
    [SerializeField] private EntityWearables _entityWearables;
    [SerializeField] private Transform _creatureTransform;
    public override void Initialize()
    {
        Logger.Log("Creature Entity Initialized", "CreatureEntity");
        base.Initialize();
    }

    protected void SetDirection(Quaternion rotation)
    {
        _creatureTransform.rotation = rotation;
    }

    protected void SetMovement(Vector2 direction, float speed)
    {
        _creatureTransform.Translate(direction * speed * Time.deltaTime);
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