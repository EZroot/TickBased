using System;

[Serializable]
public struct ItemType
{
    public string ItemName;
    public int Count;

    public ItemType(string name, int count)
    {
        this.ItemName = name;
        this.Count = count;
    }
}