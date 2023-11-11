using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class AIManager : MonoBehaviour, IServiceAIManager
    {
        [SerializeField] private bool _delayAICalls = false;
        [Range(0f,0.1f)]
        [SerializeField] private float _delayBetweenAICalculations = 0.1f;
        public List<IEnumerator> coroutineList;
        private WaitForSeconds _pathFindDelay;

        private void Start()
        {
            _pathFindDelay = new WaitForSeconds(_delayBetweenAICalculations);
            coroutineList = new List<IEnumerator>();
        }

        public void CalculateAllAIImmediately()
        {
            var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
            var creatures = creatureManager.AllCreaturesInScene;
            foreach (var creature in creatures)
            {
                if (creature is ICreatureAI)
                {
                    var ai = creature as ICreatureAI;
                    ai.CalculateAI();
                }
            }
        }
        
        public IEnumerator CalculateAllAI()
        {
            var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
            var creatures = creatureManager.AllCreaturesInScene;
            foreach (var creature in creatures)
            {
                if (creature is ICreatureAI)
                {
                    var ai = creature as ICreatureAI;
                    ai.CalculateAI();
                    yield return null;
                }
            }
        }

        public void AddCoroutineToQueue(IEnumerator coroutine)
        {
            coroutineList.Add(coroutine);
        }
        
        

        public IEnumerator ExecuteAICalculations()
        {
            foreach (var currentCoroutine in coroutineList)
            {
                yield return currentCoroutine;//StartCoroutine(currentCoroutine);

                if (_delayAICalls)
                    yield return _pathFindDelay;
            }
    
            coroutineList.Clear();
        }

        public void ExecuteAICalculationsImmediately()
        {
            foreach (var currentCoroutine in coroutineList)
            {
                currentCoroutine.MoveNext();
            }
            coroutineList.Clear();
        }
    }
}