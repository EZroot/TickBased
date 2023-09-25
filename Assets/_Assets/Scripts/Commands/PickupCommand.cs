using System.Collections;
using FearProj.ServiceLocator;
using UnityEngine;

namespace TickBased.Scripts.Commands
{
    public class PickupCommand : ICommand 
    {
        public int Priority => 1; // set your priority here
        private ICreatureEntity _currentCreatureEntity;
        private IInventoryItem _inventoryItem;
        
        public PickupCommand(ICreatureEntity currentCreature, IInventoryItem itemToPickup)
        {
            TickBased.Logger.Logger.Log($"Pickup Command Set {currentCreature.EntityData.CreatureStats.Name} " +
                       $"-> {itemToPickup.Item.ItemName}: {itemToPickup.Item.Count}");
            this._currentCreatureEntity = currentCreature;
            this._inventoryItem = itemToPickup;
        }

        public IEnumerator Execute()
        {
            if (_currentCreatureEntity.EntityData.IsDead)
                yield break;

            // Save original position
            Vector2 originalPosition = _currentCreatureEntity.CreatureTransform.position;
            var itemAsTileObj = _inventoryItem as ICreatureEntity;
            if(itemAsTileObj == null)
                Debug.LogError("FAILED TO PICKUP ITEM, it couldnt be turned in to a creature");
            var pos = itemAsTileObj.CreatureTransform.position;
            Vector2 targetPosition = pos;
            // Rotate the attacker towards the target
            Vector2 direction = targetPosition - originalPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            _currentCreatureEntity.SetDirection(Quaternion.AngleAxis(angle, Vector3.forward));
            yield return new WaitForSeconds(0.25f); // Delay for some effect or animation

            // Move to target position instantly
            _currentCreatureEntity.CreatureTransform.position = pos;


            // Apply pickup logic
            var pickedup = _inventoryItem.PickUp();
            _currentCreatureEntity.CreatureInventory.AddItem(pickedup);
            
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
    }
}