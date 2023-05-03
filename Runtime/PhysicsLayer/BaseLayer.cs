using MovementScripts;
using UnityEngine;

namespace PhysicsLayer
{
    public abstract class BaseLayer : MonoBehaviour
    {
        public abstract void OnEnter(Movement mover);
        public abstract void OnExit(Movement mover);
        public abstract void OnFixedUpdate(Movement movement);
    }
}