using System.Collections;
using UnityEngine;

namespace TickBased.Scripts.Commands
{
    public class WrestleCommand : ICommand 
    {
        public int Priority => 1; // set your priority here
        private ICreatureEntity _currentCreatureEntity;
        private ICreatureEntity _targetCreatureEntity;
        private CreatureEntityData.WrestleType _wrestleType;
        private int _attackDamage;
        private string _targetLimbName;
        public WrestleCommand(ICreatureEntity currentCreature,
            ICreatureEntity targetCreature,
            CreatureEntityData.WrestleType wrestleType,
            int attackDamage,
            string targetLimbName)
        {
            TickBased.Logger.Logger.Log($"Attack Command Set {currentCreature.EntityData.CreatureStats.Name} " +
                       $"Vs {targetCreature.EntityData.CreatureStats.Name}");
            this._attackDamage = attackDamage;
            this._currentCreatureEntity = currentCreature;
            this._targetCreatureEntity = targetCreature;
            this._wrestleType = wrestleType;
            this._targetLimbName = targetLimbName;
        }

        public IEnumerator Execute()
        {
            // Save original position
            Vector2 originalPosition = _currentCreatureEntity.CreatureTransform.position;
            Vector2 targetPosition = _targetCreatureEntity.CreatureTransform.position;
            // Rotate the attacker towards the target
            Vector2 direction = targetPosition - originalPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            _currentCreatureEntity.SetDirection(Quaternion.AngleAxis(angle, Vector3.forward));
            yield return new WaitForSeconds(0.25f); // Delay for some effect or animation

            // Move to target position instantly
            _currentCreatureEntity.CreatureTransform.position = _targetCreatureEntity.CreatureTransform.position;


            // Apply damage logic
            _targetCreatureEntity.EntityData.ActionWrestle(_wrestleType,_targetLimbName,_attackDamage);

            // Lerp back to original position
            float lerpDuration = 0.25f;
            float lerpElapsed = 0;
            while (lerpElapsed < lerpDuration)
            {
                lerpElapsed += Time.deltaTime;
                _currentCreatureEntity.CreatureTransform.position = Vector2.Lerp(_targetCreatureEntity.CreatureTransform.position, originalPosition, lerpElapsed / lerpDuration);
                yield return null;
            }

            _currentCreatureEntity.CreatureTransform.position = originalPosition;
        }

        public void ExecuteImmediately()
        {
            throw new System.NotImplementedException();
        }
    }
}