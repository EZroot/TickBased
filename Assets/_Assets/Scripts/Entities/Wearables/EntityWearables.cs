using System;
using FishNet.Component.ColliderRollback;
using TickBased.Addressables;
using UnityEngine;

public class EntityWearables : MonoBehaviour
{
    public enum WearableType
    {
        Sprite,
        Armour,
        Weapon
    }
    
    [SerializeField] private AssetLoaderSprite _entitySprite;
    [SerializeField] private AssetLoaderSprite _entitiyArmour;
    [SerializeField] private AssetLoaderSprite _entityWeapons;

    private string[] addressables = new string[3];
    public void SetWearableAddressable(WearableType wearableType, string addressableKey)
    {
        foreach(var address in addressables)
        {
            if (address == addressableKey)
            {
                return;
            }
        }
        
        switch (wearableType)
        {
            case WearableType.Sprite:
                addressables[0] = addressableKey;
                _entitySprite.LoadAssetCoroutine(addressableKey);
                break;
            case WearableType.Armour:
                addressables[1] = addressableKey;
                _entitiyArmour.LoadAssetCoroutine(addressableKey);
                break;
            case WearableType.Weapon:
                addressables[2] = addressableKey;
                _entityWeapons.LoadAssetCoroutine(addressableKey);
                break;
        }
    }

    public void HighlightSprite()
    {
        var defaultAlpha = _entitySprite.SpriteRenderer.color.a;
        _entitySprite.SpriteRenderer.color = new Color(1f, 1f, 0f, defaultAlpha);
    }

    public void UnHighlightSprite()
    {
        var defaultAlpha = _entitySprite.SpriteRenderer.color.a;
        _entitySprite.SpriteRenderer.color = new Color(1f, 1f, 1f, defaultAlpha);
    }

    public void SetSpriteAlpha(float alpha)
    {
        var color = _entitySprite.SpriteRenderer.color;
        var tmpColor = new Color(color.r, color.g, color.b, alpha);
        _entitySprite.SpriteRenderer.color = tmpColor;
    }
}
