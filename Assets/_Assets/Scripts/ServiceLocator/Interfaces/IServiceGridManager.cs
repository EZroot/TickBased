using System;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public interface IServiceGridManager : IService
    {
        event Action OnPreGridGeneration;
        event Action OnPostGridGeneration;
        public int GridSize { get; }
        public int GridTileSize { get; }
        public int GridSeed { get; }
        public GridManager.GridData Grid { get; }
        public int TileSize{ get; }
        void InitializeGridBySeed(int seed, GridManager.GridData grid);
        GridManager.Tile GetTile(int x, int y);
        void SetTileData(int x, int y, GridManager.Tile tile);
        public bool TileRaycast(int startX, int startY, int endX, int endY, out GridManager.Tile blockingTile, int thickness = 1);
        public Vector2 GridToWorld(int gridX, int gridY);
        public Vector2 GridToWorld(GridManager.GridCoordinate gridCoordinate);
        GridManager.GridCoordinate GetGridPositionFromMouse(Camera camera);
    }
}