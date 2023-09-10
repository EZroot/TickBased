using System;
using TickBased.Scripts.Commands;

namespace FearProj.ServiceLocator
{
    public interface IServiceTickManager : IService
    {
        public event Action OnTick;
        public event Action OnCommandExecuted;
        public int CurrentTick { get; }
        public TickManager.TickMode TickExecutionMode { get; set; }
        public void QueueCommand(ICommand command, int tickToExecute);
        public void ManualTick();
    }
}