using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using FishNet.Object;
using UnityEngine;

public class ObjectEntity : CreatureEntity<ObjectEntityData>
{
    public override void Start()
    {
        base.Start();

        OnEntityDataChanged += SaveEntityData;
        
        // -- No reason to check collisions on this? its not sentient --
        // var tickManager = ServiceLocator.Get<IServiceTickManager>();
        // tickManager.OnCommandExecuted +=
        //     OnCommandExecuted_CollisionUpdate; //were doin this to check collision after every movement/attack
        // OnGhostEntityMovement += OnGhostEntityMovement_CollisionUpdate; //updating our ghost so we can execute commands
    }
    void OnDestroy()
    {
        //need to remove from creature manager
        OnEntityDataChanged -= SaveEntityData;
    }

    #region NetworkBehaviour Overrides
    public override void OnStartClient()
    {
        base.OnStartClient();
        //Initialize();
    }
    #endregion
    
    public override void Initialize()
    {
        if (base.IsOwner)
        {
            base.Initialize();
            
            RPCSetEntityDataServer(_entityData);
        }
    }
    
    [ServerRpc]
    void RPCSetEntityDataServer(ObjectEntityData data)
    {
        RPCSetEntityDataClient(data);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetEntityDataClient(ObjectEntityData data)
    {
        TickBased.Logger.Logger.Log("Setting Entity Data", "RPCSetEntityDataClient");
        SetEntityData(data);
    }
}
