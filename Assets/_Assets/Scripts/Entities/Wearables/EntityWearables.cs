using System.Collections;
using System.Collections.Generic;
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

    public void SetWearableAddressable(WearableType wearableType, string addressableKey)
    {
        switch (wearableType)
        {
            case WearableType.Sprite:
                _entitySprite.LoadAssetCoroutine(addressableKey);
                break;
            case WearableType.Armour:
                _entitiyArmour.LoadAssetCoroutine(addressableKey);
                break;
            case WearableType.Weapon:
                _entityWeapons.LoadAssetCoroutine(addressableKey);
                break;
        }
    }

    public void HighlightSprite()
    {
        _entitySprite.SpriteRenderer.color = Color.yellow;
    }

    public void UnHighlightSprite()
    {
        _entitySprite.SpriteRenderer.color = Color.white;
    }
}
