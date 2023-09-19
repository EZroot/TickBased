
    using FearProj.ServiceLocator;
    using UnityEngine;

    public interface ICreatureEntity
    {
        string UniqueID { get; }
        bool IsInitialized { get; }
        CreatureEntityData EntityData { get; }
        ITileObject TileObject { get; }
        Transform CreatureTransform { get; }
        void SetDirection(Quaternion dir);
        void SetStartPosition(GridManager.GridCoordinate coordinate);
        void HighlightCreature();
        void UnHighlightCreature();
        
    }
