using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FearProj.ServiceLocator;
using FishNet.Object;
using TickBased.Scripts.Commands;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Logger = TickBased.Logger.Logger;
using Random = UnityEngine.Random;

public class AggressiveEntity : CreatureEntity<AggressiveEntityData>,ICreatureAI
{
// Serialized Fields
    [SerializeField] private int _sendCommandLimit = 1;

    // Private Fields
    private int _sendCommandLimitCounter = 0;
    private bool _canSendCommand = true;
    private Color _debugColor;
    private List<GridManager.GridCoordinate> _debugPathCoords;
    private ICreatureEntity _targetEnemy;

    #region - Unity Methods -

    public override void Start()
    {
        base.Start();
        InitializeDebugColor();
        RegisterEventHandlers();
    }

    void OnDestroy()
    {
        UnregisterEventHandlers();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        RegisterTickManager();
    }

    // Custom Initialization
    void InitializeDebugColor()
    {
        _debugColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    void RegisterEventHandlers()
    {
        OnEntityDataChanged += SaveEntityData;
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.PostTick += OnCommandExecuted_CollisionUpdate;
        //OnGhostEntityMovement += OnGhostEntityMovement_CollisionUpdate;
    }

    void UnregisterEventHandlers()
    {
        OnEntityDataChanged -= SaveEntityData;
    }

    void RegisterTickManager()
    {
        var tickmgr = ServiceLocator.Get<IServiceTickManager>();
        tickmgr.PostTick += OnPostTick_TickManager_SendCommandRefresh;
        //tickmgr.PreTick += CalculateAI;
    }

    #endregion

    public override void Initialize(string entityDataKey)
    {
        if (base.IsOwner)
        {
            base.Initialize(entityDataKey);

            RPCSetEntityDataServer(_entityData);
        }
    }

    public void CalculateAI()
    {
        if (_entityData.IsDead)
            return;

        _canSendCommand = _sendCommandLimitCounter < _sendCommandLimit;
        if (!_canSendCommand)
            return;
        
        CalculateAIPath();
        // var aiManager = ServiceLocator.Get<IServiceAIManager>();
        // aiManager.AddCoroutineToQueue(CalculateAIPath());
    }

    private bool testhide = false;

    void CalculateAIPath()
    {
        if (!IsOwner)
            return;

        if (testhide)
        {
            MoveToward(new GridManager.GridCoordinate(GridCoordinates.X + Random.Range(-10, 10),
                GridCoordinates.Y + Random.Range(-10, 10)));
        }
        if (testhide == false)
        {
            Stopwatch findEnemyWatch = new Stopwatch();
            findEnemyWatch.Start();

            _targetEnemy = FindClosestEnemy(CreatureTransform, EntityVision);

            findEnemyWatch.Stop();
            Debug.Log($"FindEnemy time: {findEnemyWatch.ElapsedMilliseconds}ms");

            if (_targetEnemy == null)
            {
                Stopwatch moveWatch = new Stopwatch();
                moveWatch.Start();

                //yield return MoveTowardEnumerator(new GridManager.GridCoordinate(GridCoordinates.X + Random.Range(-10,10),GridCoordinates.Y + Random.Range(-10,10)));
                MoveToward(new GridManager.GridCoordinate(GridCoordinates.X + Random.Range(-10, 10),
                    GridCoordinates.Y + Random.Range(-10, 10)));

                moveWatch.Stop();
                Debug.Log($"Move time: {moveWatch.ElapsedMilliseconds}ms");
            }
            else
            {
                var (isAdjacentToPlayer, targetPos) = IsAdjacentToCreature(_targetEnemy);
                if (isAdjacentToPlayer == false)
                {
                    Stopwatch moveWatch = new Stopwatch();
                    moveWatch.Start();

                    //yield return MoveTowardEnumerator(targetPos);
                    MoveToward(targetPos);

                    moveWatch.Stop();
                    Debug.Log($"Move time: {moveWatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    Debug.Log("Trying to attack creature");
                    AttackCreatureInRange(_targetEnemy);
                }
            }
        }
    }


    (bool, GridManager.GridCoordinate) IsAdjacentToCreature(ICreatureEntity creature)
    {
        //move to one of the players edges
        int xOffset = Random.Range(-1, 2); // Randomly select -1, 0, or 1
        int yOffset = Random.Range(-1, 2);

// Make sure we don't target the exact player position
        while (xOffset == 0 && yOffset == 0)
        {
            xOffset = Random.Range(-1, 2);
            yOffset = Random.Range(-1, 2);
        }

        var playerPosTarget = new GridManager.GridCoordinate(creature.TileObject.GridCoordinates.X + xOffset,
            creature.TileObject.GridCoordinates.Y + yOffset);
        bool isAdjacentToPlayer = Mathf.Abs(playerPosTarget.X - GridCoordinates.X) <= 1 &&
                                  Mathf.Abs(playerPosTarget.Y - GridCoordinates.Y) <= 1;
        return (isAdjacentToPlayer, playerPosTarget);
    }

    void MoveToward(GridManager.GridCoordinate targetCoordinate)
    {
        var path = ServiceLocator.Get<IServiceGridManager>().PathFinder;
        _debugPathCoords = path.FindPathImmediately(GridCoordinates, targetCoordinate);

        //_debugPathCoords = //path.FindPath(GridCoordinates, targetCoordinate);
        if (_debugPathCoords == null || _debugPathCoords.Count <= 1)
            return;
        
        var target = _debugPathCoords[1];
        //SetMovement(new GridManager.GridCoordinate(target.X, target.Y), true);
        // New code for the ghost character
        PathFinder.occupiedSquares.Remove(_ghostGridPosition);
        _ghostGridPosition = new GridManager.GridCoordinate(target.X,target.Y);
        PathFinder.occupiedSquares.Add(_ghostGridPosition);

        RPCSendCommandMoveServer(new GridManager.GridCoordinate(target.X, target.Y), true);
    }

    void AttackCreatureInRange(ICreatureEntity creature)
    {
        foreach (var obj in _objectsInRangeCreature)
        {
            var ent = obj as ICreatureEntity;
            if (ent.UniqueID == creature.UniqueID)
            {
                RPCSendCommandAttackServer(creature.UniqueID, CreatureEntityData.AttackType.Psychic, "Head", 10);
                break;
            }
        }
    }

    void OnPostTick_TickManager_SendCommandRefresh()
    {
        _sendCommandLimitCounter = 0;
        _canSendCommand = true;
    }


    #region Debugging

    // void OnDrawGizmos()
    // {
    //     if (!IsOwner) return;
    //     if (_debugPathCoords == null) return;
    //
    //     Gizmos.color = _debugColor;
    //     foreach (GridManager.GridCoordinate coord in _debugPathCoords)
    //     {
    //         Vector3 position = ServiceLocator.Get<IServiceGridManager>().GridToWorld(coord);
    //         Gizmos.DrawCube(position, Vector3.one);
    //     }
    // }

    #endregion

    #region -- RPC Commands --

    [ServerRpc]
    void RPCSetEntityDataServer(AggressiveEntityData data) //fucking annoying we cant send generics ;/
    {
        RPCSetEntityDataClient(data);
    }

    [ObserversRpc(BufferLast = true)]
    void RPCSetEntityDataClient(AggressiveEntityData data)
    {
        TickBased.Logger.Logger.Log("Setting Entity Data", "RPCSetEntityDataClient");
        SetEntityData(data);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RPCSendCommandWaitServer()
    {
        RPCSendCommandWaitClient();
    }

    [ObserversRpc]
    public void RPCSendCommandWaitClient()
    {
        var tick = ServiceLocator.Get<IServiceTickManager>();
        tick.QueueCommand(_entityData.UniqueID, new WaitCommand(), tick.CurrentTick);
    }

    [ServerRpc]
    public void RPCSendCommandMoveServer(GridManager.GridCoordinate newPos, bool useGhostPrediction)
    {
        RPCSendCommandMoveClient(newPos, useGhostPrediction);
    }

    [ObserversRpc]
    public void RPCSendCommandMoveClient(GridManager.GridCoordinate newPos, bool useGhostPrediction)
    {
        SetMovement(newPos, useGhostPrediction);
    }

    #endregion
}