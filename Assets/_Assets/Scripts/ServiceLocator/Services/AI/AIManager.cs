using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class AIManager : MonoBehaviour, IServiceAIManager
    {
        private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();

        public void AddCoroutineToQueue(IEnumerator coroutine)
        {
            coroutineQueue.Enqueue(coroutine);
        }

        public IEnumerator ExecuteAICalculations()
        {
            while (coroutineQueue.Count > 0)
            {
                IEnumerator currentCoroutine = coroutineQueue.Dequeue();
                yield return StartCoroutine(currentCoroutine);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}