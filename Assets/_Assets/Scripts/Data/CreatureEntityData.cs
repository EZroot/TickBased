using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CreatureEntityData : EntityData
{
    public ClientStats ClientStats;
    public CreatureStats CreatureStats;
    public CreatureSprites CreatureSprites;
    
    public CreatureEntityData() : base()
    {
        
    }

    public void SetCreatureSprites(CreatureSprites sprites)
    {
        Logger.Log($"Setting sprites...", "CreatureEntityData");
        CreatureSprites = new CreatureSprites(sprites);
    }
    public void SetClientStats(ClientStats stats)
    {
        Logger.Log($"From CreatueEntData: {stats.ClientId} {stats.Username}");
        ClientStats = new ClientStats(stats);
    }

    public void SetCreatureStats(CreatureStats stats)
    {
        CreatureStats = new CreatureStats(stats);
    }
}

[System.Serializable]
public class CreatureSprites
{
    public string SpriteAddressableKey;
    public string ArmourAddressableKey;
    public string WeaponAddressableKey;
    
    public CreatureSprites()
    {

    }

    public CreatureSprites(CreatureSprites stats)
    {
        this.SpriteAddressableKey = stats.SpriteAddressableKey;
        this.ArmourAddressableKey = stats.ArmourAddressableKey;
        this.WeaponAddressableKey = stats.WeaponAddressableKey;
    }
    public CreatureSprites(string spriteAddressableKey, string armourAddressableKey, string weaponAddressableKey)
    {
        this.SpriteAddressableKey = spriteAddressableKey;
        this.ArmourAddressableKey = armourAddressableKey;
        this.WeaponAddressableKey = weaponAddressableKey;
    }
}

[System.Serializable]
public class CreatureStats
{
    public string Name;
    public float MovementSpeed;

    public CreatureStats()
    {

    }

    public CreatureStats(CreatureStats stats)
    {
        this.Name = stats.Name;
        this.MovementSpeed = stats.MovementSpeed;
    }
    public CreatureStats(string name, float movementSpeed)
    {
        this.Name = name;
        this.MovementSpeed = movementSpeed;
    }

    public void SetMovementSpeed(float movementSpeed)
    {
        this.MovementSpeed = movementSpeed;
    }
}

[System.Serializable]
public class ClientStats
{
    public int ClientId;
    public string Username;

    public ClientStats()
    {

    }

    public ClientStats(ClientStats stats)
    {
        this.ClientId = stats.ClientId;
        this.Username = stats.Username;
    }
    public ClientStats(int id, string username)
    {
        this.ClientId = id;
        this.Username = username;
    }
}