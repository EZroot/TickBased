using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AssetLoaderSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public void LoadAssetCoroutine(string addressableKey)
    {
        StartCoroutine(LoadAssetEnumerator(addressableKey));
    }
    
    private IEnumerator LoadAssetEnumerator(string addressableKey)
    {
        var task = LoadAssetAsync(addressableKey);
        yield return new WaitUntil(() => task.Status.IsCompleted());
        var sprite = task.GetAwaiter().GetResult();
        _spriteRenderer.sprite = sprite;
    }
    
    private async UniTask<Sprite> LoadAssetAsync(string addressableKey)
    {
        var asset = await Addressables.LoadAssetAsync<Sprite>(addressableKey);
        return asset;
    }
}
