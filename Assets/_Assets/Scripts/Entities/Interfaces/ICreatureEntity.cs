
    using UnityEngine;

    public interface ICreatureEntity
    {
        string UniqueID { get; }
        CreatureEntityData EntityData { get; }
        Transform CreatureTransform { get; }
        void SetDirection(Quaternion dir);
    }
