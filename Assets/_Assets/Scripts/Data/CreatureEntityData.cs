using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CreatureEntityData : EntityData
{
    public bool IsPlayer;
    public bool IsDead;
    public ClientStats ClientStats;
    public CreatureStats CreatureStats;
    public CreatureSprites CreatureSprites;
    
    public CreatureEntityData() : base()
    {
        
    }

    public void SetCreatureSprites(CreatureSprites sprites)
    {
        TickBased.Logger.Logger.Log($"Setting sprites...", "CreatureEntityData");
        CreatureSprites = new CreatureSprites(sprites);
    }
    
    public void SetClientStats(ClientStats stats)
    {
        TickBased.Logger.Logger.Log($"From CreatueEntData: {stats.ClientId} {stats.Username}");
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
    
    [System.Serializable]
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
    [System.Serializable]
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
    public int Strength;
    public FactionData Faction;
    public CreatureLimbsData[] CreatureLimbs;
    public CreatureMoodleData CreatureMoodle;
    public CreatureStatData[] CreatureAbilityStats;
    public CreatureStats()
    {

    }

    public CreatureStats(CreatureStats stats)
    {
        this.Name = stats.Name;
        this.CreatureLimbs = stats.CreatureLimbs;
        this.CreatureMoodle = stats.CreatureMoodle;
        this.CreatureAbilityStats = stats.CreatureAbilityStats;
    }

    
    [System.Serializable]
    public struct CreatureStatData
    {
        public CreatureStatType CreatureStatType;
        public int Level;
        public double CurrentXP;
        public double RequiredXP;

        public void AddXp(double xpToAdd)
        {
            
        }

        public void RemoveXP(double xpToRemove)
        {
            
        }
        
        public void CalculateRequiredXp()
        {
            //do logic
        }

        void LevelUp()
        {
            
        }

        void LevelDown()
        {
            
        }
    }

    [System.Serializable]
    public enum CreatureStatType
    {
        Strength,
        Perception,
        Endurance,
        Charisma,
        Intelligence,
        Agility,
        Luck
    }

    [System.Serializable]
    public struct CreatureMoodleData
    {
        public MoodleData Moodle;
    }

    [System.Serializable]
    public struct MoodleData
    {
        public MoodleMoodType CurrentMood;
        public float MoodDuration; //how long mood will last
        public int MoodImpact; //effect of mood on creature?
    }

    [System.Serializable]
    public enum MoodleMoodType
    {
        Depressed,
        Sad,
        Neutral,
        Happy,
        Gleeful
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
    
    [System.Serializable]
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
    
    [System.Serializable]
    public struct FactionData
    {
        public string FactionName;
        public string FactionDescription;
        public FactionType FactionType;
    }

    [System.Serializable]
    public enum FactionType
    {
        PlayerFaction,
        AggressiveEnemyFaction
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