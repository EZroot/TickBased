using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityData
{
    public string ID;
    public string IDHash;

    public EntityData()
    {
        // if(_id != string.Empty)
        //     _idHash = CryptoUtils.GenerateSHA256Hash(_id);
    }
}
