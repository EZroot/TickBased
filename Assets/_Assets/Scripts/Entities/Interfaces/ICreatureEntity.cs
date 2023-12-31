﻿
    using FearProj.ServiceLocator;
    using UnityEngine;

    public interface ICreatureEntity
    {
        string UniqueID { get; }
        bool IsInitialized { get; }
        bool CanInteract { get; }
        int EntityVision { get; }
        bool IsDead { get; }
        CreatureEntityData EntityData { get; }
        CreatureInventory CreatureInventory { get; }
        ITileObject TileObject { get; }
        Transform CreatureTransform { get; }
        Transform Transform { get; }
        void SetDirection(Quaternion dir);
        void SetStartPosition(GridManager.GridCoordinate coordinate, GridManager.TileState tileState);
        void HighlightCreature();
        void UnHighlightCreature();
        ICreatureEntity FindClosestEnemy(Transform creatureTransform, float detectionRadius);
    }
