using System;
using TickBased.Scripts.Commands;

namespace FearProj.ServiceLocator
{
    public interface IServiceTickManager : IService
    {
        public event Action OnTick;
        public void QueueCommand(ICommand command);
        public void ManualTick();
    }
}