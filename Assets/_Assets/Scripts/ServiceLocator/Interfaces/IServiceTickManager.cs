using System;
using System.Collections;
using TickBased.Scripts.Commands;

namespace FearProj.ServiceLocator
{
    public interface IServiceTickManager : IService
    {
        public event Action OnTick;
        public event TickManager.CommandReceivedHandler OnCommandReceived;
        public event Action OnCommandExecuted;
        public int CurrentTick { get; }
        public TickManager.TickMode TickExecutionMode { get; set; }
        public void QueueCommand(string creatureUniqueID, ICommand command, int tickToExecute);
        public void ManualTick();
        public void StopManualTickCoroutine();
        bool CheckIfCreatureCanIssueCommandThisTick(string creatureUniqueID);
    }
}