using PhysicsLayer;
using UnityEngine;

namespace MovementScripts
{
    public class PhysicLayerHandler
    {
        bool hasLayer;
        BaseLayer currentLayer;

        public void OnFixedUpdate(Movement movement)
        {
            if (hasLayer)
                currentLayer.OnFixedUpdate(movement);
        }
        public void TryEnterPhysicsLayer(Movement movement, Collision other)
        {
            if (!other.gameObject.TryGetComponent(out BaseLayer baseLayer))
                return;
            if (hasLayer) currentLayer.OnExit(movement);
            currentLayer = baseLayer;
            hasLayer = true;
            baseLayer.OnEnter(movement);
        }

        public void TryExitPhysicsLayer(Movement movement, Collision other)
        {
            if (!other.gameObject.TryGetComponent(out BaseLayer baseLayer)) return;
            if (baseLayer == currentLayer)
                baseLayer.OnExit(movement);
            currentLayer = null;
            hasLayer = false;
        }
    }
}