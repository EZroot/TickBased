using System;
using TickBased.Scripts.Commands;

namespace FearProj.ServiceLocator
{
    public interface IServiceTickManager : IService
    {
        public event Action PreTick;
        public event Action PostTick;
        public event TickManager.CommandReceivedHandler OnCommandReceived;
        public event Action OnCommandExecuted;
        public int CurrentTick { get; }
        public bool IsExecutingTick { get; }
        public TickManager.TickMode TickExecutionMode { get; set; }
        public TickManager.TickModeRealTimeType TickModeRealTime { get; set; }
        public TickManager.CommandExecutionMode CommandExecutionType { get; set; }
        public void QueueCommand(string creatureUniqueID, ICommand command, int tickToExecute);
        public void ManualTick();
        public void StopManualTickCoroutine();
        bool CheckIfCreatureCanIssueCommandThisTick(string creatureUniqueID);
    }
}