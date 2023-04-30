using CollsionBasedMovement;
using UnityEngine;

namespace PhysicsLayer
{
    public abstract class BaseLayer : MonoBehaviour
    {
        public abstract void OnEnter(Movement moveStatsManager);
        public abstract void OnExit(Movement moveStatsManager);
        public abstract void OnFixedUpdate(Movement movement);
    }
}