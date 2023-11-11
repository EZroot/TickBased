using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FearProj.ServiceLocator
{
    public interface IServiceAIManager : IService
    {
        IEnumerator CalculateAllAI();
        void CalculateAllAIImmediately();
        void AddCoroutineToQueue(IEnumerator coroutine);
        IEnumerator ExecuteAICalculations();
        void ExecuteAICalculationsImmediately();
    }
}