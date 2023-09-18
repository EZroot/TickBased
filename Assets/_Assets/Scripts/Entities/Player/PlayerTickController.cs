using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FearProj.ServiceLocator;
using FishNet.Object;
using TickBased.Scripts.Commands;
using UnityEngine;

public class PlayerTickController : NetworkBehaviour
{
    //initially we want to get all the players, (and future creatures when theyre done)
    //we want to make sure the player each player has sent 1 command
    //if each player has sent 1 command, then we will run the tick
    
    private Dictionary<string, bool> _playersCommandStatus = new Dictionary<string, bool>(); // Key is playerID, value is whether they've issued a command this tick

    private float _lastCommandCheckTime = 0;
    [SerializeField] private float _timeLimitForCommand = 1.0f; // 1 second to issue a command
    private bool _isInitialized = false;
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsHost == false || IsOwner == false)
            return;

        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.OnCommandReceived += OnTickManager_CommandRecieved;
        tickManager.PostTick += OnTick_ResetForceCommandTimer;
        _isInitialized = true;
    }

    void Update()
    {
        if (_isInitialized == false || IsHost == false || IsOwner == false)
            return;

        _lastCommandCheckTime += Time.deltaTime;
        if (_lastCommandCheckTime >= _timeLimitForCommand)
        {
            ForcePlayersToSendCommand();
            _lastCommandCheckTime = 0;
        }
    }

    void OnTick_ResetForceCommandTimer()
    {
        _lastCommandCheckTime = 0;
    }

    void OnTickManager_CommandRecieved(string uniqueID, ICommand command, int tickExecution)
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();

        //if its manually, well handle it through ending turns? since this will auto trigger when everyone has sent 1 command
        if (tickManager.TickExecutionMode == TickManager.TickMode.Manual)
            return;

        //initialize our player list if a new player has joined
        PopulatePlayerStatusDictionary();

        //this player has registered a command
        _playersCommandStatus[uniqueID] = true;
        TickBased.Logger.Logger.Log($"PlayerCommandStatus {uniqueID} = true", "PlayerTickController");
        //make sure we have our players counted
        if (_playersCommandStatus.Keys.Count == creatureManager.AllPlayersInScene.Count)
        {
            TickBased.Logger.Logger.Log($"All Player Command Status's have been recorded", "PlayerTickController");

            //check if all status's are true
            var allPlayersSentACommand = true;
            foreach (var pair in _playersCommandStatus)
            {
                if (!pair.Value) // If the value is false
                {
                    allPlayersSentACommand = false;
                    break; // Exit loop as soon as one player has not sent a command
                }
            }
 
            if (allPlayersSentACommand)
            {
                //has to be real time
                if (tickManager.TickExecutionMode == TickManager.TickMode.RealTime)
                {
                    //Reset everything to false, we dont clear it 
                    _playersCommandStatus = _playersCommandStatus.ToDictionary(x => x.Key, x => false);
                    RPCStartManualTick();
                }
            }
        }
    }

    void ForcePlayersToSendCommand()
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        if (tickManager.TickExecutionMode == TickManager.TickMode.Manual)
            return;
        
        PopulatePlayerStatusDictionary();
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        foreach (var kvp in _playersCommandStatus)
        {
            if(kvp.Value == true)
                continue;
            TickBased.Logger.Logger.Log($"Forcing {kvp.Key} to send command", "PlayerTickController");

            var id = kvp.Key;
            var player = creatureManager.GetPlayer(id);
            if(player == null || player.IsInitialized == false)
                continue;
            player.RPCSendCommandWaitServer();
        }
    }
    
    void PopulatePlayerStatusDictionary()
    {
        var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
        if (_playersCommandStatus.Keys.Count != creatureManager.AllPlayersInScene.Count)
        {
            _playersCommandStatus.Clear();
            foreach (var player in creatureManager.AllPlayersInScene)
            {
                var id = player.UniqueID;
                _playersCommandStatus.TryAdd(id, false);
            }
        }
    }

    [ServerRpc]
    void RPCStartManualTick()
    {
        RPCStartManualTickClient();
    }
    
    [ObserversRpc]
    void RPCStartManualTickClient()
    {
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.ManualTick();
    }
}
