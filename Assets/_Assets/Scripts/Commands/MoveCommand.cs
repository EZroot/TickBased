using System.Collections;
using UnityEngine;

namespace TickBased.Scripts.Commands
{
    public class MoveCommand : ICommand
    {
        public int Priority => 1; // set your priority here
        private Vector2 targetPosition;
        private Transform entityTransform;

        public MoveCommand(Transform entityTransform, Vector2 targetPosition)
        {
//            Logger.Log($"Movement Command Set {entityTransform.root.gameObject.name} {targetPosition}");
            this.entityTransform = entityTransform;
            this.targetPosition = targetPosition;
        }

        public IEnumerator Execute()
        {
            float duration = .1f;
            float elapsed = 0;
            Vector2 startPosition = entityTransform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                entityTransform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / duration);
                yield return null;
            }

            entityTransform.position = targetPosition;
        }
    }
}