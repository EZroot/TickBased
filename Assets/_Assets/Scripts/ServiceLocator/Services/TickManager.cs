using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickBased.Scripts.Commands;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class TickManager : MonoBehaviour, IServiceTickManager
    {
        public event Action OnTick;
        private List<ICommand> commandQueue = new List<ICommand>();
        private int currentTick = 0;
        private WaitForSeconds _commandDelay = new WaitForSeconds(0.25f);

        private bool _isExecutingCommands = false;
        
        public void QueueCommand(ICommand command)
        {
            Logger.Log($"Added command {command.GetType()}", "TickManager");
            commandQueue.Add(command);
        }
        
        public void ManualTick()
        {
            if (_isExecutingCommands)
                return;
            OnTick?.Invoke();
            ExecuteCommands();
        }

        private void ExecuteCommands()
        {
            // Sort commands by priority, if needed
            //commandQueue = commandQueue.OrderBy(c => c.Priority).ToList();

            // Use a MonoBehaviour to start the Coroutine
            StartCoroutine(ExecuteCommandsCoroutine());
        }

        private IEnumerator ExecuteCommandsCoroutine()
        {
            _isExecutingCommands = true;
            foreach (var command in commandQueue)
            {
                yield return StartCoroutine(command.Execute());
                yield return _commandDelay;
            }

            commandQueue.Clear();
            _isExecutingCommands = false;
        }
    }
}