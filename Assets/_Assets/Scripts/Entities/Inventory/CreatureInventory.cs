using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CreatureInventory //this will be inventory controller, creature data should have itemtype as a list thatll hold the data for this
//on load, this should populate from the data, and this will be used to take items out / put items in the creatures inventory
{
    [SerializeField] private List<ItemType> _inventoryItems;

    public CreatureInventory()
    {
        _inventoryItems = new List<ItemType>();
    }

    public CreatureInventory(List<ItemType> startingItems)
    {
        _inventoryItems = startingItems;
    }

    public void AddItem(ItemType item)
    {
        // Check if the item already exists in the inventory.
        for (int i = 0; i < _inventoryItems.Count; i++)
        {
            if (_inventoryItems[i].ItemName == item.ItemName)
            {
                // Update the count of the existing item.
                var existingItem = _inventoryItems[i];
                existingItem.Count += item.Count;
                _inventoryItems[i] = existingItem;
                return;
            }
        }

        // If the item does not exist in the inventory, add it.
        _inventoryItems.Add(item);
    }
    
    public bool TryGetItem(string itemName, out ItemType item)
    {
        item = new ItemType("", 0);
        foreach (var it in _inventoryItems)
        {
            if (it.ItemName == itemName)
            {
                item = it;
                return true;
            }
        }
        return false;
    }
    
    public void RemoveItem(ItemType item, int amountToRemove)
    {
        for (var index = 0; index < _inventoryItems.Count; index++)
        {
            var it = _inventoryItems[index];
            if (it.ItemName == item.ItemName)
            {
                var tmp = _inventoryItems[index];
                var newItem = new ItemType(tmp.ItemName, tmp.Count - amountToRemove);
                if (newItem.Count <= 0)
                {
                    _inventoryItems.RemoveAt(index);
                    break;
                }
                _inventoryItems[index] = newItem;
                break;
            }
        }
    }
    
    public bool TryRemoveItem(string itemName, out ItemType item)
    {
        item = new ItemType("", 0);
        for (var index = 0; index < _inventoryItems.Count; index++)
        {
            var it = _inventoryItems[index];
            if (it.ItemName == itemName)
            {
                item = new ItemType(it.ItemName, it.Count);
                _inventoryItems.RemoveAt(index);
                return true;
            }
        }

        return false;
    }
}