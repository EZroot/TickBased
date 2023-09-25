using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickBased.Scripts.Commands;
using Unity.VisualScripting;
using UnityEngine;

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

        //temporarily serilaizable so we can mess with it in debug
        [SerializeField] private TickMode _tickMode;
        [SerializeField] private TickModeRealTimeType _tickModeRealTimeType;
        [SerializeField] private CommandExecutionMode _commandExecutionMode;
        private Coroutine _realTimeTickCoroutine;
        private bool _isExecutingTick;
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

        public TickModeRealTimeType TickModeRealTime => _tickModeRealTimeType;

        void Start()
        {
            // if (_tickMode == TickMode.RealTime)
            // {
            //     _realTimeTickCoroutine = StartCoroutine(RealTimeTickCoroutine());
            // }
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
            _isExecutingTick = true;
            PreTick?.Invoke(); //collisions and other actions should happen after movement?

            TickBased.Logger.Logger.Log($"Manual Tick called {_currentTick} ", "TickManager");
            yield return ExecuteCommands();
            _currentTick++;
            var aiManager = ServiceLocator.Get<IServiceAIManager>();
            yield return aiManager.ExecuteAICalculations();
            PostTick?.Invoke(); //collisions and other actions should happen after movement?
            _isExecutingTick = false;
        }

        private IEnumerator ExecuteCommands()
        {
            TickBased.Logger.Logger.Log("Executing Command", "TickManager");
            if (_commandQueue.ContainsKey(_currentTick))
            {
                List<Coroutine> runningCoroutines = new List<Coroutine>();
                const int chunkSize = 4; // Choose an appropriate chunk size

                foreach (var creatureCommands in _commandQueue[_currentTick])
                {
                    TickBased.Logger.Logger.Log("Starting Execute Command Coroutine", "TickManager");
                    List<ICommand> commands = creatureCommands.Value;
                    
                    if (_commandExecutionMode == CommandExecutionMode.Sequentially)
                    {
                        yield return ExecuteCommandsCoroutine(commands);
                    }
                    else if (_commandExecutionMode == CommandExecutionMode.Concurrently)
                    {
                        for (int i = 0; i < commands.Count; i += chunkSize)
                        {
                            List<ICommand> commandChunk = commands.Skip(i).Take(chunkSize).ToList();
                            Coroutine cr = StartCoroutine(ExecuteCommandsCoroutine(commandChunk));
                            runningCoroutines.Add(cr);
                        }
                    }
                }

                // // Wait for all coroutines to complete
                if (_commandExecutionMode == CommandExecutionMode.Concurrently)
                {
                    foreach (Coroutine cr in runningCoroutines)
                    {
                        yield return cr;
                    }
                }

                //_commandQueue.Remove(_currentTick);
            }
        }

        private IEnumerator ExecuteCommandsCoroutine(List<ICommand> commandsToExecute)
        {
            commandsToExecute.Add(new WaitCommand()); //sending this to update the player collision after last action
            foreach (var command in commandsToExecute)
            {
                TickBased.Logger.Logger.Log($"Executing command: {command.GetType()}", "TickManager");

                yield return command.Execute();
                OnCommandExecuted?.Invoke();

                if (TickExecutionMode == TickMode.Manual)
                    yield return _commandDelay;
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