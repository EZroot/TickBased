using System.Collections;
using UnityEngine;

namespace TickBased.Scripts.Commands
{
    public class WaitCommand : ICommand
    {
        public int Priority => 1;

        //Intentionally empty, since we dont want to do anything
        public WaitCommand()
        {
        }

        public IEnumerator Execute()
        {
            yield return null;
        }
    }
}