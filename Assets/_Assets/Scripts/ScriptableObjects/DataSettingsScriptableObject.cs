using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataSettings", menuName = "EZROOT/GameSettings/DataSetting")]
public class DataSettingsScriptableObject : ScriptableObject
{
    [SerializeField] private string _addressableEntityDataSettingLabel;
    [SerializeField] private string[] dataPath;
    [SerializeField] private string[] assetPath;

    public string AddressableEntityDataSettingLabel => _addressableEntityDataSettingLabel;
    public string[] DataPath => dataPath;
    public string[] AssetPath => assetPath;
}
