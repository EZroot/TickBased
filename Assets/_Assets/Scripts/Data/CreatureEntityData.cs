using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CreatureEntityData : EntityData
{
    public string UniqueID;
    public bool IsPlayer;
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

    public void ActionDamage(AttackType attackType, string limbName, int damage)
    {
        //for the attack type, we want to do specialties here
        //like apply extra damage, or cut off the limb if dmg is double hp or something?, or something similar
        for (var i = 0; i < CreatureStats.CreatureLimbs.Length; i++)
        {
            if (CreatureStats.CreatureLimbs[i].LimbName == limbName)
            {
                CreatureStats.CreatureLimbs[i].Health -= damage;
                return;
            }
        }
    }
    
    public void ActionWrestle(WrestleType wrestleType, string limbName, int damage)
    {
        //for the attack type, we want to do specialties here
        //like apply extra damage, or cut off the limb if dmg is double hp or something?, or something similar
        for (var i = 0; i < CreatureStats.CreatureLimbs.Length; i++)
        {
            if (CreatureStats.CreatureLimbs[i].LimbName == limbName)
            {
                CreatureStats.CreatureLimbs[i].Health -= damage;
                return;
            }
        }
    }

    public enum AttackType
    {
        Unarmed,
        Blunt,
        Bludgeon,
        Slashing,
        Piercing,
        Elemental,
        Magic,
        Ranged, 
        Crushing, 
        Poison,
        Psychic,
        Energy, 
        Explosive
    }
    
    public enum WrestleType
    {
        Grab,           // Grab a part of the opponent
        Pin,            // Pin opponent to the ground
        Choke,          // Choking hold
        JointLock,      // Lock a joint to restrict movement
        Takedown,       // Take the opponent to the ground
        Throw,          // Throw the opponent
        Strangle,       // Cut off air supply
        BreakHold,      // Break free from opponent's hold
        TwistLimb,      // Twist an arm or leg
        Bite,           // Bite the opponent
        Gouge,          // Gouge eyes or other sensitive area
        Headbutt,       // Strike with the head
        Shove           // Push the opponent
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
    public CreatureLimbsData[] CreatureLimbs;
    
    public CreatureStats()
    {

    }

    public CreatureStats(CreatureStats stats)
    {
        this.Name = stats.Name;
        this.MovementSpeed = stats.MovementSpeed;
        this.CreatureLimbs = stats.CreatureLimbs;
    }

    public CreatureStats(string name, float movementSpeed, CreatureLimbsData[] limbs)
    {
        this.Name = name;
        this.MovementSpeed = movementSpeed;
        this.CreatureLimbs = limbs;
    }

    public CreatureLimbsData GetLimb(string limbName)
    {
        foreach (var limb in CreatureLimbs)
        {
            if (limb.LimbName == limbName)
                return limb;
        }
        return new CreatureLimbsData();
    }
    
    public CreatureLimbsData[] GetLimbs(CreatureLimbType limbType)
    {
        List<CreatureLimbsData> tmp = new List<CreatureLimbsData>();
        foreach (var limb in CreatureLimbs)
        {
            if (limb.LimbType == limbType)
                tmp.Add(limb);
        }
        return tmp.ToArray();
    }

    public void SetMovementSpeed(float movementSpeed)
    {
        this.MovementSpeed = movementSpeed;
    }

    [System.Serializable]
    public struct CreatureLimbsData
    {
        public string LimbName;
        public int Health;
        public int Armour;
        public bool IsAttached;
        public CreatureLimbType LimbType;
    }
    
    public enum CreatureLimbType
    {
        Head,
        Eyes,
        Ears,
        Hands,
        Arm,
        Leg,
        Torso,
        Groin
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