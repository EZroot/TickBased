using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerEntityData", menuName = "EZROOT/EntityData/PlayerEntityData")]
public class PlayerDataSO : DataSO
{
    [SerializeField] private PlayerEntityData _playerEntityData;
    public PlayerEntityData EntityData => _playerEntityData;
}
