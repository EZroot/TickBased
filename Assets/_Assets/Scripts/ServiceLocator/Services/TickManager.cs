using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TickBased.Scripts.Commands;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Logger = TickBased.Logger.Logger;

namespace FearProj.ServiceLocator
{
    public class TickManager : MonoBehaviour, IServiceTickManager
    {
        public enum TickMode
        {
            RealTime,
            Manual
        }

        public enum TickModeRealTimeType
        {
            WaitForAllPlayers,
            ForcePlayerAction
        }

        public enum CommandExecutionMode
        {
            Sequentially,
            Concurrently
        }

        //records the current tick, the unique id of the creature, and the command it sent
        private Dictionary<int, Dictionary<string, List<ICommand>>> _commandQueue =
            new Dictionary<int, Dictionary<string, List<ICommand>>>();

        private int _currentTick = 0;
        private WaitForSeconds _commandDelay = new WaitForSeconds(0.25f);
        private WaitCommand _waitCommand;

        //temporarily serilaizable so we can mess with it in debug
        [SerializeField] private TickMode _tickMode;
        [SerializeField] private TickModeRealTimeType _tickModeRealTimeType;
        [SerializeField] private CommandExecutionMode _commandExecutionMode;
        [Header("This can cause issues when switching from forced wait to player cmd wait")]
        [SerializeField] private bool _usePostTickDelay;
        [SerializeField] private float _postTickDelay = 1f;
        private Coroutine _realTimeTickCoroutine;
        private bool _isExecutingTick;
        private WaitForSeconds _tickDelayWaitForSeconds;
        public delegate void CommandReceivedHandler(string uniqueCreatureID, ICommand command, int executionTick);

        public event CommandReceivedHandler OnCommandReceived;
        public event Action OnCommandExecuted;
        public event Action PreTick;
        public event Action PostTick;

        public int CurrentTick => _currentTick;

        public bool IsExecutingTick => _isExecutingTick;

        public TickMode TickExecutionMode
        {
            get => _tickMode;
            set
            {
                if (_tickMode != value)
                {
                    if (_tickMode == TickMode.RealTime && _realTimeTickCoroutine != null)
                    {
                        StopCoroutine(_realTimeTickCoroutine);
                    }

                    _tickMode = value;
                }
            }
        }

        public TickModeRealTimeType TickModeRealTime
        {
            get { return _tickModeRealTimeType; }
            set => _tickModeRealTimeType = value;
        }

        public CommandExecutionMode CommandExecutionType
        {
            get => _commandExecutionMode;
            set => _commandExecutionMode = value;
        }


        private void OnGUI()
        {
            GUI.Box(new Rect(0,0,205,110), "TickManager");
            GUI.Label(new Rect(10, 15, 200, 20), $"{TickExecutionMode.ToString()}");
            GUI.Label(new Rect(10, 35, 200, 20), $"{TickModeRealTime.ToString()}");
            GUI.Label(new Rect(10, 55, 200, 20), $"{CommandExecutionType.ToString()}");
            GUI.Label(new Rect(10, 75, 200, 20), $"Tick: {CurrentTick}");
        }
        
        void Start()
        {
            _waitCommand = new WaitCommand();
            _tickDelayWaitForSeconds = new WaitForSeconds(_postTickDelay);
        }

        public void QueueCommand(string creatureUniqueID, ICommand command, int tickToExecute)
        {
            if (_commandQueue.ContainsKey(_currentTick))
                TickBased.Logger.Logger.Log($"QueueCommand: CommandQueue: {_commandQueue[_currentTick].Count}",
                    "TickManager");

            if (!_commandQueue.ContainsKey(tickToExecute))
            {
                _commandQueue[tickToExecute] = new Dictionary<string, List<ICommand>>();
                TickBased.Logger.Logger.Log($"CommandQueue: Creating dictionary at tick {tickToExecute} ",
                    "TickManager");
            }

            if (!_commandQueue[tickToExecute].ContainsKey(creatureUniqueID))
            {
                _commandQueue[tickToExecute][creatureUniqueID] = new List<ICommand>();
            }

            _commandQueue[tickToExecute][creatureUniqueID].Add(command);
            TickBased.Logger.Logger.Log(
                $"QueueCommand: [{tickToExecute}][{creatureUniqueID}]: {_commandQueue[tickToExecute][creatureUniqueID].Count} ",
                "TickManager");

            OnCommandReceived?.Invoke(creatureUniqueID, command, tickToExecute);
        }

        public void ManualTick()
        {
            if (_isExecutingTick)
                return;
            StopManualTickCoroutine();
            _realTimeTickCoroutine = StartCoroutine(ManualTickEnumerator());
        }

        public void StopManualTickCoroutine()
        {
            if (_realTimeTickCoroutine != null)
                StopCoroutine(_realTimeTickCoroutine);
        }

        public IEnumerator ManualTickEnumerator()
        {
            if (_isExecutingTick)
                yield break;
            _isExecutingTick = true;
            PreTick?.Invoke(); //collisions and other actions should happen after movement?
           // TickBased.Logger.Logger.Log($"Manual Tick called {_currentTick} ", "TickManager");
            yield return ExecuteCommands();
            _currentTick++;
            //var aiManager = ServiceLocator.Get<IServiceAIManager>();
           // aiManager.CalculateAllAI();

            //aiManager.ExecuteAICalculationsImmediately(); this is slower lol
            //yield return aiManager.ExecuteAICalculations();
            var pt = new Stopwatch();
            pt.Start();
            PostTick?.Invoke(); //collisions and other actions should happen after movement?
            pt.Stop();
            // Logger.LogError($"POST TICK {pt.ElapsedMilliseconds}ms");
            var aiManager = ServiceLocator.Get<IServiceAIManager>();
            //yield return aiManager.CalculateAllAI();
            //pt.Reset();
            //pt.Start();
            aiManager.CalculateAllAIImmediately();
            //pt.Stop();

            if (_usePostTickDelay)
                yield return _tickDelayWaitForSeconds;
            // Logger.LogError($"AI CALC {pt.ElapsedMilliseconds}ms");

            _isExecutingTick = false;
        }
        
        private IEnumerator ExecuteCommands()
        {
            //TickBased.Logger.Logger.Log("Executing Command", "TickManager");
            if (_commandQueue.ContainsKey(_currentTick))
            {
                List<Coroutine> runningCoroutines = new List<Coroutine>();
                List<IEnumerator> runningEnumeartors = new List<IEnumerator>();
                foreach (var creatureCommands in _commandQueue[_currentTick])
                {
                   // TickBased.Logger.Logger.Log("Starting Execute Command Coroutine", "TickManager");
                    List<ICommand> commands = creatureCommands.Value;
                    int batchSize = 3; // or any other number that works well
                    if (_commandExecutionMode == CommandExecutionMode.Sequentially)
                    {
                       // yield return ExecuteCommandsCoroutine(commands);
                       for (int i = 0; i < commands.Count; i += batchSize)
                       {
                           List<ICommand> batch = commands.Skip(i).Take(batchSize).ToList();
                           var cr = ExecuteCommandsCoroutine(batch);
                           runningEnumeartors.Add(cr);
                       }
                    }
                    else if (_commandExecutionMode == CommandExecutionMode.Concurrently)
                    {
                        foreach (var cmd in commands)
                        {
                            cmd.ExecuteImmediately();
                            yield return null;
                        }

                        var pp = new Stopwatch();
                        pp.Start();
                        //OnCommandExecuted?.Invoke();
                        pp.Stop();
//                        TickBased.Logger.Logger.LogError($"ON CMD EXECUTE {pp.ElapsedMilliseconds}ms");
                        // for (int i = 0; i < commands.Count; i += batchSize)
                        // {
                        //     List<ICommand> batch = commands.Skip(i).Take(batchSize).ToList();
                        //     var cr = StartCoroutine(ExecuteCommandsCoroutine(batch));
                        //     runningCoroutines.Add(cr);
                        // }
                        yield return null;

                    }
    
                }

                // // Wait for all coroutines to complete
                if (_commandExecutionMode == CommandExecutionMode.Sequentially)
                {
                    Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>(runningEnumeartors);
                    while(coroutineQueue.Count > 0)
                    {
                        yield return coroutineQueue.Dequeue();
                        yield return null;
                    }
                }
            }
        }


        private IEnumerator ExecuteCommandsCoroutine(List<ICommand> commandsToExecute)
        {
            commandsToExecute.Add(_waitCommand); //sending this to update the player collision after last action
            foreach (var command in commandsToExecute)
            {

                yield return command.Execute();
                OnCommandExecuted?.Invoke();

                if (TickExecutionMode == TickMode.Manual)
                    yield return _commandDelay;
            }
        }

        private void ExecuteCommandsImmediately(List<ICommand> commandsToExecute)
        {
            commandsToExecute.Add(_waitCommand); //sending this to update the player collision after last action
            foreach (var command in commandsToExecute)
            {

                command.ExecuteImmediately();
                OnCommandExecuted?.Invoke();
                //
                // if (TickExecutionMode == TickMode.Manual)
                //     yield return _commandDelay;
            }
            
        }
        public bool CheckIfCreatureCanIssueCommandThisTick(string creatureUniqueID)
        {
            if (TickExecutionMode == TickMode.RealTime)
            {
                var commandAlreadyExists = CheckIfCreatureCommandExists(_currentTick, creatureUniqueID);
                TickBased.Logger.Logger.Log($"Can Issue Command: {commandAlreadyExists}", "TickManager");
                return !commandAlreadyExists; //command exists so we cant issue a command
            }

            return true;
        }

        bool CheckIfCreatureCommandExists(int tick, string uniqueID)
        {
            return _commandQueue.ContainsKey(tick) && _commandQueue[tick].ContainsKey(uniqueID);
        }
    }
}