using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallEntityData : CreatureEntityData
{
    public int RequiredWork;
    public int CurrentWork;
    
    public WallEntityData() : base()
    {

    }
}