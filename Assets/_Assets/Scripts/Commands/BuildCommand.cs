using System.Collections;
using FearProj.ServiceLocator;
using UnityEngine;

namespace TickBased.Scripts.Commands
{
    public class BuildCommand : ICommand 
    {
        public int Priority => 1; // set your priority here
        private ICreatureEntity _currentCreatureEntity;
        private IBuildableObject _buildableObject;
        private int _buildWorkToAdd;
        public BuildCommand(ICreatureEntity currentCreature, IBuildableObject itemToBuild, int buildWorkToAdd)
        {
            TickBased.Logger.Logger.Log($"Build Command Set {currentCreature.EntityData.CreatureStats.Name} " +
                       $"-> {itemToBuild.WorkRequired}:");
            this._currentCreatureEntity = currentCreature;
            this._buildableObject = itemToBuild;
            this._buildWorkToAdd = buildWorkToAdd;
        }

        public IEnumerator Execute()
        {
            if (_currentCreatureEntity.EntityData.IsDead)
                yield break;

            // Save original position
            Vector2 originalPosition = _currentCreatureEntity.CreatureTransform.position;
            var itemAsTileObj = _buildableObject as ICreatureEntity;
            var pos = itemAsTileObj.CreatureTransform.position;
            Vector2 targetPosition = pos;
            // Rotate the attacker towards the target
            Vector2 direction = targetPosition - originalPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            _currentCreatureEntity.SetDirection(Quaternion.AngleAxis(angle, Vector3.forward));
            yield return new WaitForSeconds(0.25f); // Delay for some effect or animation

            // Move to target position instantly
            _currentCreatureEntity.CreatureTransform.position = pos;

            _buildableObject.AddWork(_buildWorkToAdd);
            // Apply damage logic
           // var pickedup = _inventoryItem.PickUp();
           // _currentCreatureEntity.CreatureInventory.AddItem(pickedup);
            
            // Lerp back to original position
            float lerpDuration = 0.25f;
            float lerpElapsed = 0;
            while (lerpElapsed < lerpDuration)
            {
                lerpElapsed += Time.deltaTime;
                _currentCreatureEntity.CreatureTransform.position = Vector2.Lerp(pos, originalPosition, lerpElapsed / lerpDuration);
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