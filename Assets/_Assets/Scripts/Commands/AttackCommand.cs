using System.Collections;
using UnityEngine;

namespace TickBased.Scripts.Commands
{
    public class AttackCommand : ICommand 
    {
        public int Priority => 1; // set your priority here
        private ICreatureEntity _currentCreatureEntity;
        private ICreatureEntity _targetCreatureEntity;
        private int _attackDamage;
        public AttackCommand(ICreatureEntity currentCreature,
            ICreatureEntity targetCreature,
            int attackDamage)
        {
            Logger.Log($"Attack Command Set {currentCreature.EntityData.CreatureStats.Name} " +
                       $"Vs {targetCreature.EntityData.CreatureStats.Name}");
            this._attackDamage = attackDamage;
            this._currentCreatureEntity = currentCreature;
            this._targetCreatureEntity = targetCreature;
        }

        public IEnumerator Execute()
        {
            // Animation or effect for attacking
            // e.g. Rotate the attacker towards the target
            Vector2 direction = _targetCreatureEntity.CreatureTransform.position - _currentCreatureEntity.CreatureTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            _currentCreatureEntity.SetDirection(Quaternion.AngleAxis(angle, Vector3.forward));

            // yield return new WaitForSeconds(0.2f); // Delay for some effect or animation

            // Apply damage logic
            // e.g. targetEntity.TakeDamage(damage);
            _targetCreatureEntity.EntityData.TakeDamage(_attackDamage);

            yield return null;
        }
    }
}