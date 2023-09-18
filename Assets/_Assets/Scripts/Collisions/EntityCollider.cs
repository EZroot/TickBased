using UnityEngine;

public class EntityCollider : MonoBehaviour
{
    [SerializeField] private GameObject _creatureEntity;
    private ICreatureEntity _creatureEntityInterface;

    void Start()
    {
        _creatureEntityInterface = _creatureEntity.GetComponent<ICreatureEntity>();
    }
    
    public ICreatureEntity GetEntity()
    {
        return _creatureEntityInterface;
    }
}
