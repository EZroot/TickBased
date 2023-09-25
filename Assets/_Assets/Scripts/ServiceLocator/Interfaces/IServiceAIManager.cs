using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FearProj.ServiceLocator
{
    public interface IServiceAIManager : IService
    {
        public void AddCoroutineToQueue(IEnumerator coroutine);
        IEnumerator ExecuteAICalculations();
    }
}