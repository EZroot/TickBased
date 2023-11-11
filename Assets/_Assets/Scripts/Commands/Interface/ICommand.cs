using System.Collections;

namespace TickBased.Scripts.Commands
{
    public interface ICommand
    {
        int Priority { get; }
        IEnumerator Execute();
        void ExecuteImmediately();
    }
}