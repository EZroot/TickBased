using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataSettings", menuName = "EZROOT/GameSettings/DataSetting")]
public class DataSettingsScriptableObject : ScriptableObject
{
    [SerializeField] private string _addressableEntityDataSettingLabel;
    [SerializeField] private string dataPath;
    [SerializeField] private string assetPath;
    [Header("This folder can be used to store test assets, like maps. It will get copied on the build to asset paths above on build")]
    [SerializeField] private string localDevAssetsToCopy;

    public string AddressableEntityDataSettingLabel => _addressableEntityDataSettingLabel;
    public string DataPath => dataPath;
    public string AssetPath => assetPath;
    public string LocalDevAssetsToCopy => localDevAssetsToCopy;
}
