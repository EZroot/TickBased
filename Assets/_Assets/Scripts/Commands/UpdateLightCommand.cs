using System.Collections;

namespace TickBased.Scripts.Commands
{
    public class UpdateLightCommand : ICommand
    {
        public int Priority { get; }
        public IEnumerator Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}