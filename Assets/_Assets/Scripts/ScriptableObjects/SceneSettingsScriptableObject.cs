using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneSettings", menuName = "EZROOT/GameSettings/SceneSettings")]
public class SceneSettingsScriptableObject : ScriptableObject
{
    [SerializeField] private string _bootstrapSceneName;
    [SerializeField] private string _lobbySceneName;
    [SerializeField] private string _gameSceneName;

    public string BootstrapSceneName { get => _bootstrapSceneName; }
    public string LobbySceneName { get => _lobbySceneName; }
    public string GameSceneName { get => _gameSceneName; }

}
