namespace FearProj.ServiceLocator
{
    public interface IInventoryItem
    {
        public bool CanPickup { get; }
        public ItemType Item { get; }
        public ItemType PickUp();
        public void Drop(GridManager.GridCoordinate position);
    }
}