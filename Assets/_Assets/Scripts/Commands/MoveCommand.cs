using System.Collections;
using FearProj.ServiceLocator;
using UnityEngine;

namespace TickBased.Scripts.Commands
{
    public class MoveCommand : ICommand
    {
        public int Priority => 1; // set your priority here
        private  GridManager.GridCoordinate targetPosition;
        private ICreatureEntity _creatureEntity;

        public MoveCommand(ICreatureEntity creatureEntity, GridManager.GridCoordinate targetPosition)
        {
            TickBased.Logger.Logger.Log($"Movement Command Set {creatureEntity.EntityData.ID} {targetPosition.X},{targetPosition.Y}");
            this._creatureEntity = creatureEntity;
            this.targetPosition = targetPosition;
        }

        public IEnumerator Execute()
        {
            if (_creatureEntity.EntityData.IsDead)
                yield break;
            
            float duration = .1f;
            float elapsed = 0;

            Vector2 startPosition = _creatureEntity.CreatureTransform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var floatVal = new Vector2(targetPosition.X, targetPosition.Y);
                _creatureEntity.CreatureTransform.position = Vector2.Lerp(startPosition, floatVal, elapsed / duration);
                yield return null;
            }
            var targ = new Vector2(targetPosition.X, targetPosition.Y);

            _creatureEntity.CreatureTransform.position = targ;
            
            var gridManager = ServiceLocator.Get<IServiceGridManager>();
            
            var lightManager = ServiceLocator.Get<IServiceLightManager>();

            //lightManager.ResetAccumulator();
            var x = (int)(targ.x / gridManager.TileSize);
            var y = (int)(targ.y / gridManager.TileSize);
            
            lightManager.UpdateLightSourcePosition(_creatureEntity.UniqueID, x,y);
            
            //lightManager.AddRadialLight($"derp_{x}", x+8,y, 4, new Color(0f,0f,0f,0f));
            //lightManager.ApplyAccumulatedLight();
            //lightManager.SetTileLight(x, y, 0f);
        }
    }
}