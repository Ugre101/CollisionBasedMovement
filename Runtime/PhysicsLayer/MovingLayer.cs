using MovementScripts;

namespace PhysicsLayer
{
    public class MovingLayer : BaseLayer
    {
        
        public override void OnEnter(Movement mover)
        {
            mover.transform.SetParent(transform);
        }

        public override void OnExit(Movement mover)
        {
            mover.transform.SetParent(null);
        }

        public override void OnFixedUpdate(Movement movement)
        {
        }
    }
}