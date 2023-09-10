using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TickBased.Scripts.Commands;
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
        
        private Dictionary<int, List<ICommand>> _commandQueue = new Dictionary<int, List<ICommand>>();
        private int _currentTick = 0;
        private WaitForSeconds _commandDelay = new WaitForSeconds(0.25f);
        private WaitForSeconds _realTimeTickDelay = new WaitForSeconds(1f);

        //temporarily serilaizable so we can mess with it in debug
        [SerializeField] private TickMode _tickMode;
        private bool _isExecutingCommands = false;
        private Coroutine _realTimeTickCoroutine;

        public event Action OnTick;
        public event Action OnCommandExecuted;
        public int CurrentTick => _currentTick;
        public TickMode TickExecutionMode
        {
            get => _tickMode;
            set
            {
                if (_tickMode != value)
                {
                    if(_tickMode == TickMode.RealTime && _realTimeTickCoroutine != null)
                    {
                        StopCoroutine(_realTimeTickCoroutine);
                    }

                    // Start new real-time coroutine if switching to real-time mode
                    if(value == TickMode.RealTime)
                    {
                        _realTimeTickCoroutine = StartCoroutine(RealTimeTickCoroutine());
                    }
                }
                _tickMode = value;
            }
        }
        
        private void Start()
        {
            if (_tickMode == TickMode.RealTime)
            {
                _realTimeTickCoroutine = StartCoroutine(RealTimeTickCoroutine());
            }
        }
        
        public void QueueCommand(ICommand command, int tickToExecute)
        {
            Logger.Log($"Added command {command.GetType()}", "TickManager");
            if (!_commandQueue.ContainsKey(tickToExecute))
            {
                _commandQueue[tickToExecute] = new List<ICommand>();
            }
            _commandQueue[tickToExecute].Add(command);        }
        
        public void ManualTick()
        {
            if (_isExecutingCommands)
                return;
            
            Logger.Log($"Manual Tick called {_currentTick} ", "TickManager");
            ExecuteCommands();
            OnTick?.Invoke(); //collisions and shit should happen after movement?
            _currentTick++;
        }

        private void ExecuteCommands()
        {
            // Sort commands by priority, if needed
            //commandQueue = commandQueue.OrderBy(c => c.Priority).ToList();

            // Use a MonoBehaviour to start the Coroutine
            if (_commandQueue.ContainsKey(_currentTick))
            {
                StartCoroutine(ExecuteCommandsCoroutine(_commandQueue[_currentTick]));
                _commandQueue.Remove(_currentTick);
            }
        }

        private IEnumerator RealTimeTickCoroutine()
        {
            while (_tickMode == TickMode.RealTime)
            {
                if (!_isExecutingCommands)
                {
                    ManualTick();
                }
                yield return _realTimeTickDelay;
            }
        }

        private IEnumerator ExecuteCommandsCoroutine(List<ICommand> commandsToExecute)
        {
            _isExecutingCommands = true;
            foreach (var command in commandsToExecute)
            {
                yield return StartCoroutine(command.Execute());
                OnCommandExecuted?.Invoke();

                if (TickExecutionMode == TickMode.Manual)
                    yield return _commandDelay;
            }
            _isExecutingCommands = false;
        }
    }
}