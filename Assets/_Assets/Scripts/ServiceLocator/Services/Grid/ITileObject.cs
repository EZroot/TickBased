namespace FearProj.ServiceLocator
{
    public interface ITileObject
    {
        public GridManager.GridCoordinate GridCoordinates { get; }
        void OnTileEnter();
        void OnTileExit();

        void SetGridCoordinates(int x, int y);
    }
}